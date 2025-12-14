using ManagedCropoutSampleProject.Interactable;
using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.Core;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI.Tasks;

[UClass]
public partial class UFarmingTask : UCropoutBaseTask
{
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector Crop { get; set; }

    private FName _tagState;

    public override async void ReceiveExecuteAI(AAIController ownerController, APawn controlledPawn)
    {
        AActor cropActor = UBTFunctionLibrary.GetBlackboardValueAsActor(this, Crop);
        
        if (cropActor == null)
        {
            FinishExecute(false);
            return;
        }

        if (!cropActor.ActorHasTag("Ready") && !cropActor.ActorHasTag("Harvest"))
        {
            UBTFunctionLibrary.SetBlackboardValueAsObject(this, Crop, null);
            FinishExecute(true);
            return;
        }
        
        if (cropActor is not AInteractable interactable) 
        {
            FinishExecute(false);
            return;
        }

        _tagState = cropActor.Tags[1];
        float duration = interactable.Interact();

        if (controlledPawn is not IVillager villager)
        {
            FinishExecute(false);
            return;
        }
        
        villager.PlayWorkAnim(duration);
        await Task.Delay(TimeSpan.FromSeconds(duration)).ConfigureWithUnrealContext();

        if (_tagState == "Harvest")
        {
            FinishExecute(false);
        }
        else
        {
            UBTFunctionLibrary.SetBlackboardValueAsObject(this, Crop, null);
            FinishExecute(true);
        }
    }
}