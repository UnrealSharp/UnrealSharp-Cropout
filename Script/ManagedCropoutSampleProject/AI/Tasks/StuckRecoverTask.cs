using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI.Tasks;

[UClass]
public partial class UStuckRecoverTask : UCropoutBaseTask
{
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector RecoveryPosition { get; set; }
    
    protected override void ReceiveExecuteAI_Implementation(AAIController ownerController, APawn controlledPawn)
    {
        FVector newLocation = UBTFunctionLibrary.GetBlackboardValueAsVector(this, RecoveryPosition);
        bool success = controlledPawn.SetActorLocation(newLocation, false, out FHitResult result, false);
        FinishExecute(success);
    }
}