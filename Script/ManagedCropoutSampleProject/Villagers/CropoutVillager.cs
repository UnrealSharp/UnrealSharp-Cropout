using ManagedCropoutSampleProject.Core.Save;
using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.Villagers;
using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject;

[UClass]
public class ACropoutVillager : APawn, IVillager
{
    // Components
    [UProperty(DefaultComponent = true, RootComponent = true)]
    UCapsuleComponent Capsule { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(Capsule))]
    USkeletalMeshComponent SkeletalMesh { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(SkeletalMesh))]
    UStaticMeshComponent Tool { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(SkeletalMesh))]
    USkeletalMeshComponent Hat { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(SkeletalMesh))]
    USkeletalMeshComponent Hair { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(Capsule))]
    UDecalComponent Decal { get; set; }
    
    [UProperty(DefaultComponent = true)]
    UFloatingPawnMovement FloatingPawnMovement { get; set; }
    
    // Public variables
    // TODO: Change to bitmask field to match original (https://github.com/UnrealSharp/UnrealSharp/issues/287)
    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadOnly, Category = "Resource")]
    public TSet<EResourceType> ResourcesHeld { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadOnly, Category = "Resource")]
    public int Quantity { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadOnly, Category = "Job Profile")]
    public AActor TargetRef { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadOnly, Category = "Job Profile")]
    public FName NewJob { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadOnly, Category = "Job Profile")]
    public UBehaviorTree ActiveBehaviour { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadOnly, Category = "Job Profile")]
    public UAnimMontage WorkAnim { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadOnly, Category = "Job Profile")]
    public UStaticMesh? TargetTool { get; set; }
 
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public FName CurrentJob { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public TMap<FName, FJob> FallbackJobMap { get; set; }
    
    // This is where villagers are assigned new jobs.
    // The Event Action is sent from the player and passes along the target for the villager.
    // Read the Tag from the target and try to change jobs based on that tag name.
    public void Action(AActor? actor)
    {
        if (actor == null || !actor.IsValid || actor.Tags.Count == 0)
        {
            return;
        }
        
        ChangeJob(actor.Tags[0]);

        IGameInstance gameInstance = (IGameInstance) UGameplayStatics.GetGameInstance(this);
        gameInstance.UpdateAllVillagers();
    }

    public void ChangeJob(FName newJob)
    {
      
    }

    public void PlayWorkAnim(float delay)
    {
        
    }

    public float PlayDeliverAnim(float progressBuilding)
    {
        // TODO
        return 0;
    }

    public void ReturnToDefaultBt()
    {
        // TODO
    }

    public float ProgressBuilding(float investedTime)
    {
        // TODO
        return 0;
    }

    private void Eat()
    {
        // TODO
    }

    private void ResetJobState()
    {
        StopJob();

        Hat.SkeletalMesh = null;
        Tool.SetVisibility(false);
        Tool.SetStaticMesh(null);
        TargetTool = null;
    }

    private void StopJob()
    {
        Tool.SetVisibility(false);
        SkeletalMesh.AnimInstance.Montage_StopGroupByName(0, "DefaultGroup");
        Quantity = 0;
        
        var aiController = AIHelperLibrary.GetAIController(this);

        if (SystemLibrary.IsValid(aiController))
        {
            aiController.StopMovement();
        }
    }

    private void HairPick()
    {
        
    }
}