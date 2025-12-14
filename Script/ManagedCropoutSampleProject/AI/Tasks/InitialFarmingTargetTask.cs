using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.Interactable.Buildings;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI.Tasks;

[UClass]
public partial class UInitialFarmingTargetTask : UCropoutBaseTask
{
    [UProperty(PropertyFlags.EditInstanceOnly, Category = "Target Classes")]
    public partial FBlackboardKeySelector Key_ResourceClass { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly, Category = "Target Classes")]
    public partial FBlackboardKeySelector Key_CollectionClass { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector Key_Resource { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector Key_TownHall { get; set; }

    public override void ReceiveExecuteAI(AAIController ownerController, APawn controlledPawn)
    {
        AActor resource = UBTFunctionLibrary.GetBlackboardValueAsActor(this, Key_Resource);

        if (resource == null)
        {
            UGameplayStatics.GetAllActorsOfClassWithTag(typeof(ACrop), "Ready", out IList<AActor> resources);
            
            if (resources.Count == 0)
            {
                FinishExecute(false);
                return;
            }

            FVector pawnLocation = controlledPawn.ActorLocation;
            AActor nearestActor = UGameplayStatics.FindNearestActor(pawnLocation, resources, out float distance);
            
            UBTFunctionLibrary.SetBlackboardValueAsObject(this, Key_Resource, nearestActor);
        }
        
        UBTFunctionLibrary.SetBlackboardValueAsClass(this, Key_ResourceClass, typeof(ACrop));
        UBTFunctionLibrary.SetBlackboardValueAsClass(this, Key_CollectionClass, typeof(ABuilding));

        ATownHall townHall = UGameplayStatics.GetActorOfClass<ATownHall>();
        UBTFunctionLibrary.SetBlackboardValueAsObject(this, Key_TownHall, townHall);
            
        FinishExecute(true);
    }
}