using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.Interactable;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI.Tasks;

[UClass]
public partial class UTransferResourceTask : UCropoutBaseTask
{
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector GiveTo { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector TakeFrom { get; set; }

    protected override void ReceiveExecute_Implementation(AActor ownerActor)
    {
        AActor takeFromActor = UBTFunctionLibrary.GetBlackboardValueAsActor(this, TakeFrom);
        AActor giveToActor = UBTFunctionLibrary.GetBlackboardValueAsActor(this, GiveTo);

        if (takeFromActor == null || giveToActor == null)
        {
            FinishExecute(false);
            return;
        }
        
        if (takeFromActor is not IResourceInterface takeFromInterface)
        {
            FinishExecute(false);
            return;
        }
        
        if (giveToActor is not IResourceInterface giveToInterface)
        {
            FinishExecute(false);
            return;
        }
        
        takeFromInterface.RemoveResource(out KeyValuePair<EResourceType, int> pair);
        giveToInterface.AddResource(pair);
        FinishExecute(true);
    }
}