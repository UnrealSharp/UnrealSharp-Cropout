using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.Interactable.Buildings;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI.Tasks;

[UClass]
public class UInitialFarmingTargetTask : UCropoutBaseTask
{
    [UProperty(PropertyFlags.EditInstanceOnly, Category = "Target Classes")]
    public FBlackboardKeySelector Key_ResourceClass { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly, Category = "Target Classes")]
    public FBlackboardKeySelector Key_CollectionClass { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public FBlackboardKeySelector Key_Resource { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public FBlackboardKeySelector Key_TownHall { get; set; }

    protected override void ReceiveExecuteAI(AAIController ownerController, APawn controlledPawn)
    {
        AActor resource = UBTFunctionLibrary.GetBlackboardValueAsActor(this, Key_Resource);

        if (resource == null)
        {
            UGameplayStatics.GetAllActorsOfClassWithTag<ACrop>("Ready", out IList<ACrop> resources);
            
            if (resources.Count == 0)
            {
                FinishExecute(false);
                return;
            }

            FVector pawnLocation = controlledPawn.ActorLocation;
            AActor nearestActor = UGameplayStatics.FindNearestActor(pawnLocation, (IList<AActor>) resources, out float distance);
            
            UBTFunctionLibrary.SetBlackboardValueAsObject(this, Key_Resource, nearestActor);
        }
        
        UBTFunctionLibrary.SetBlackboardValueAsClass(this, Key_ResourceClass, typeof(ACrop));
        UBTFunctionLibrary.SetBlackboardValueAsClass(this, Key_CollectionClass, typeof(ABuilding));

        ATownHall townHall = UGameplayStatics.GetActorOfClass<ATownHall>();
        UBTFunctionLibrary.SetBlackboardValueAsObject(this, Key_TownHall, townHall);
            
        FinishExecute(true);
    }
}