using ManagedCropoutSampleProject.Interactable;
using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI.Tasks;

[UClass]
public partial class UProgressConstructionTask : UCropoutBaseTask
{
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector TargetBuild { get; set; }
    
    public override async void ReceiveExecuteAI(AAIController ownerController, APawn controlledPawn)
    {
        ACropoutVillager villager = (ACropoutVillager) controlledPawn;
        villager.PlayWorkAnim(1.0f);
        
        await Task.Delay(TimeSpan.FromSeconds(1.0f)).ConfigureWithUnrealContext();
        
        AActor foundTarget = UBTFunctionLibrary.GetBlackboardValueAsActor(this, TargetBuild);

        if (foundTarget is not AInteractable interactable)
        {
            FinishExecute(false);
            return;
        }

        float workTime = interactable.Interact();
        if (workTime <= 0.0f)
        {
            UBTFunctionLibrary.SetBlackboardValueAsObject(this, TargetBuild, null);
        }

        FinishExecute(true);
    }
}