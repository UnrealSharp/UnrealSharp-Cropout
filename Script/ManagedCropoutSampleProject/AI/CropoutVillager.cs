using ManagedCropoutSampleProject.AI;
using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.Core.Save;
using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.Villagers;
using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.AnimGraphRuntime;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject;

[UClass]
public partial class ACropoutVillager : APawn, IVillager, IResourceInterface
{
    [UProperty(DefaultComponent = true, RootComponent = true)]
    public UCapsuleComponent Capsule { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(Capsule))]
    public USkeletalMeshComponent SkeletalMesh { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(SkeletalMesh), AttachmentSocket = "hand_rSocket")]
    public UStaticMeshComponent Tool { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(SkeletalMesh), AttachmentSocket = "headSocket")]
    public USkeletalMeshComponent Hat { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(SkeletalMesh))]
    public USkeletalMeshComponent Hair { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(Capsule))]
    public UDecalComponent Decal { get; set; }
    
    [UProperty(DefaultComponent = true)]
    public UFloatingPawnMovement FloatingPawnMovement { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public AActor? Target { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public FName CurrentJob { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected UDataTable JobsDataTable { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected UStaticMesh CrateMesh { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected UAnimMontage PutDownAnim { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public int Quantity { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    protected UStaticMesh? TargetTool { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    protected UBehaviorTree ActiveBehaviorTree { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    protected UAnimMontage ActiveWorkAnim { get; set; }

    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected IList<TSoftObjectPtr<USkeletalMesh>> AllHairMeshes { get; set; }

    [UProperty(PropertyFlags.BlueprintReadOnly)]
    protected EResourceType ResourcesHeld { get; set; }

    protected override void BeginPlay()
    {
        FVector offset = new FVector(0.0f, 0.0f, Capsule.ScaledCapsuleHalfHeight);
        AddActorWorldOffset(offset, false, out _, false);

        SystemLibrary.SetTimer(Eat, 24.0f, true);
        
        ChangeJob("Idle");
        AssignDefaultHat();
        
        base.BeginPlay();
    }

    public override void ConstructionScript()
    {
        SkeletalMesh.SetCustomPrimitiveDataFloat(0, MathLibrary.RandomFloat().ToFloat());
        SkeletalMesh.SetCustomPrimitiveDataFloat(1, MathLibrary.RandomFloat().ToFloat());
        
        base.ConstructionScript();
    }

    // This is where villagers are assigned new jobs.
    // The Event Action is sent from the player and passes along the target for the villager.
    // Read the Tag from the target and try to change jobs based on that tag name.
    public void Action(AActor? actor)
    {
        if (actor == null || !actor.IsValid)
        {
            return;
        }
        
        Target = actor;

        if (actor.Tags.Count == 0)
        {
            return;
        }
        
        ChangeJob(actor.Tags[0]);

        IGameInstance gameInstance = (IGameInstance) UGameplayStatics.GameInstance;
        gameInstance.UpdateAllVillagers();
    }

    public void ChangeJob(FName newJob)
    {
        if (!JobsDataTable.HasRow(newJob))
        {
            return;
        }
        
        CurrentJob = newJob;
        FJobs job = JobsDataTable.FindRow<FJobs>(CurrentJob);
        
        if (Tags.IndexOf(newJob) == -1)
        {
            Tags.Add(newJob);
        }
        
        Tags.Clear();
        Tags.Add(newJob);
        
        ResetJobState();
        AssignVisuals(job);
    }

    public float PlayDeliverAnim()
    {
        PlayVillagerAnim(PutDownAnim, 1.0f);
        return 1.0f;
    }

    public void PlayWorkAnim(float delay)
    {
        PlayVillagerAnim(ActiveWorkAnim, delay);
        SetToolMesh(TargetTool);
    }

    private async void PlayVillagerAnim(UAnimMontage montage, float length)
    {
        UPlayMontageCallbackProxy playMontage = UPlayMontageCallbackProxy.CreateProxyObjectForPlayMontage(SkeletalMesh, montage, 1.0f);
        playMontage.OnInterrupted += OnMontageFinished;
        playMontage.OnCompleted += OnMontageFinished;
        
        await Task.Delay(TimeSpan.FromSeconds(length)).ConfigureWithUnrealContext();
        
        UAnimInstance animInstance = SkeletalMesh.AnimInstance;
        animInstance.Montage_StopGroupByName(0, "DefaultGroup");
    }

    [UFunction]
    private void OnMontageFinished(FName notifyName)
    {
        SetToolMesh(null);
    }

    public float ProgressBuilding(float timeRemaining)
    {
        throw new NotImplementedException();
    }

    public void ReturnToDefaultBT()
    {
        throw new NotImplementedException();
    }

    void ResetJobState()
    {
        Hat.SkeletalMesh = null;
        Hat.SetVisibility(false);
        
        SetToolMesh(null);
        
        StopJob();
    }

    async void AssignDefaultHat()
    {
        Random rand = new Random();
        TSoftObjectPtr<USkeletalMesh> randomHat = AllHairMeshes[rand.Next(0, AllHairMeshes.Count)];
        
        USkeletalMesh hat = await randomHat.LoadAsync();
        Hat.SkeletalMesh = hat;
        Hat.SetCustomPrimitiveDataFloat(0, rand.Next());
    }

    async void AssignVisuals(FJobs jobInfo)
    {
        List<FSoftObjectPath> softObjectPaths = new List<FSoftObjectPath>(4);
        softObjectPaths.Add(jobInfo.BehaviorTree.SoftObjectPath);
        softObjectPaths.Add(jobInfo.WorkAnim.SoftObjectPath);
        softObjectPaths.Add(jobInfo.Hat.SoftObjectPath);
        softObjectPaths.Add(jobInfo.Tool.SoftObjectPath);
        
       await softObjectPaths.LoadAsync();
       
       if (jobInfo.BehaviorTree.SoftObjectPath.Object is UBehaviorTree behaviorTree)
       {
           AAIController controller = AIHelperLibrary.GetAIController(this);
           
           if (controller == null)
           {
               LogCropout.LogError("Villager has no AIController");
               return;
           }
           
           controller.RunBehaviorTree(behaviorTree);
           
           ActiveBehaviorTree = behaviorTree;

           UBlackboardComponent? blackboardComponent = AIHelperLibrary.GetBlackboard(controller);
           blackboardComponent.SetValueAsObject("Target", Target);
       }
       
       if (jobInfo.WorkAnim.SoftObjectPath.Object is UAnimMontage workAnim)
       {
           ActiveWorkAnim = workAnim;
       }
       
       if (jobInfo.Hat.SoftObjectPath.Object is USkeletalMesh hat)
       {
           Hat.SkeletalMesh = hat;
           Hat.SetVisibility(true);
       }
         
       if (jobInfo.Tool.SoftObjectPath.Object is UStaticMesh tool)
       {
           TargetTool = tool;
       }
    }

    [UFunction]
    void Eat()
    {
        ACropoutGameMode gameMode = World.GameModeAs<ACropoutGameMode>();
        KeyValuePair<EResourceType, int> resource = new KeyValuePair<EResourceType, int>(EResourceType.Food, 3);
        gameMode.RemoveTargetResource(resource);
    }
    
    void StopJob()
    {
        SetToolMesh(null);
        SkeletalMesh.AnimInstance.Montage_StopGroupByName(0, "DefaultGroup");

        AAIController controller = AIHelperLibrary.GetAIController(this);
        if (controller != null)
        {
            controller.StopMovement();
        }
        
        Quantity = 0;
    }

    public void RemoveTargetResource(KeyValuePair<EResourceType, int> resource)
    {
        throw new NotImplementedException();
    }

    public void AddResource(KeyValuePair<EResourceType, int> resource)
    {
        ResourcesHeld = resource.Key;
        Quantity = resource.Value;
        
        SetToolMesh(CrateMesh);
    }
    
    public void RemoveResource(out KeyValuePair<EResourceType, int> resource)
    {
        EResourceType cachedResource = ResourcesHeld;
        int cachedAmount = Quantity;

        ResourcesHeld = EResourceType.None;
        Quantity = 0;
        
        SetToolMesh(null);
        resource = new KeyValuePair<EResourceType, int>(cachedResource, cachedAmount);
    }

    public IDictionary<EResourceType, int> GetCurrentResources()
    {
        throw new NotImplementedException();
    }

    public bool CheckResource(EResourceType resourceType, out int amount)
    {
        throw new NotImplementedException();
    }

    private void SetToolMesh(UStaticMesh? mesh)
    {
        if (mesh != null)
        {
            Tool.SetStaticMesh(mesh);
            Tool.SetVisibility(true);
        }
        else
        {
            Tool.SetStaticMesh(null);
            Tool.SetVisibility(false);
        }
    }
}