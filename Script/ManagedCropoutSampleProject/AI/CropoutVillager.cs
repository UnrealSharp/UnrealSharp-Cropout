using ManagedCropoutSampleProject.AI;
using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.Core.Save;
using ManagedCropoutSampleProject.Villagers;
using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject;

[UClass]
public partial class ACropoutVillager : APawn, IVillager
{
    [UProperty(DefaultComponent = true, RootComponent = true)]
    public UCapsuleComponent Capsule { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(Capsule))]
    public USkeletalMeshComponent SkeletalMesh { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(SkeletalMesh))]
    public UStaticMeshComponent Tool { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(SkeletalMesh))]
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

    protected override void BeginPlay()
    {
        FVector offset = new FVector(0.0f, 0.0f, Capsule.ScaledCapsuleHalfHeight);
        AddActorWorldOffset(offset, false, out _, false);

        SystemLibrary.SetTimer(Eat, 24.0f, true);
        
        ChangeJob("Idle");
        AssignDefaultHat();
        
        base.BeginPlay();
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
        throw new NotImplementedException();
    }

    public void PlayWorkAnim(float delay)
    {
        throw new NotImplementedException();
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
        
        Tool.SetStaticMesh(null);
        TargetTool = null;
        
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
    }
    
    void StopJob()
    {
        Tool.SetVisibility(false);
        SkeletalMesh.AnimInstance.Montage_StopGroupByName(0, "DefaultGroup");

        AAIController controller = AIHelperLibrary.GetAIController(this);
        if (controller != null)
        {
            controller.StopMovement();
        }
        
        Quantity = 0;
    }
}