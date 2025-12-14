using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.Interactable;
using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI.Tasks;

[UClass]
public partial class UWorkTask : UCropoutBaseTask
{
    public UWorkTask()
    {
        DelayMultiplier = 1.0f;
    }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector DelayKey { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector GiveTo { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector TakeFrom { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial float DelayMultiplier { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    private partial AInteractable? Interactable { get; set; }
    
    public override void ReceiveExecuteAI(AAIController ownerController, APawn controlledPawn)
    {
        AActor takeFromActor = UBTFunctionLibrary.GetBlackboardValueAsActor(this, TakeFrom);
        AActor giveToActor = UBTFunctionLibrary.GetBlackboardValueAsActor(this, GiveTo);
        
        if (takeFromActor == null || giveToActor == null)
        {
            FinishExecute(false);
            return;
        }

        if (takeFromActor is not AInteractable interactable)
        {
            FinishExecute(false);
            return;
        }
        
        StartTakingResources(controlledPawn, takeFromActor, giveToActor, interactable);
    }

    async void StartTakingResources(APawn controlledPawn, AActor takeFromActor, AActor giveToActor, AInteractable interactable)
    {
        Interactable = interactable;
        Interactable.PlayWobble(controlledPawn.ActorLocation);
        float delay = Interactable.Interact();
        
        if (controlledPawn is not IVillager villager)
        {
            FinishExecute(false);
            return;
        }
        
        villager.PlayWorkAnim(delay);

        await Task.Delay(TimeSpan.FromSeconds(delay)).ConfigureWithUnrealContext();
        
        if (giveToActor is not IResourceInterface giveToInterface)
        {
            FinishExecute(false);
            return;
        }
        
        if (takeFromActor is not IResourceInterface takeFromInterface)
        {
            FinishExecute(false);
            return;
        }
        
        takeFromInterface.RemoveResource(out KeyValuePair<EResourceType, int> resource);
        giveToInterface.AddResource(resource);
        
        FinishExecute(true);
    }

    public override void ReceiveAbortAI(AAIController ownerController, APawn controlledPawn)
    {
        if (Interactable != null && Interactable.IsValid())
        {
            Interactable.StopWobble();
        }

        base.ReceiveAbortAI(ownerController, controlledPawn);
    }
}