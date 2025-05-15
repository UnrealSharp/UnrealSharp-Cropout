using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.Villagers;
using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI.Tasks;

[UClass]
public class UDeliverResourceTask : UCropoutBaseTask
{
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public FBlackboardKeySelector GiveTo { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public FBlackboardKeySelector TakeFrom { get; set; }
    
    protected override async void ReceiveExecuteAI(AAIController ownerController, APawn controlledPawn)
    {
        AActor takeFromActor = UBTFunctionLibrary.GetBlackboardValueAsActor(this, TakeFrom);
        AActor giveToActor = UBTFunctionLibrary.GetBlackboardValueAsActor(this, GiveTo);
        
        if (takeFromActor == null || giveToActor == null)
        {
            FinishExecute(false);
            return;
        }
        
        if (controlledPawn is not IVillager villager)
        {
            FinishExecute(false);
            return;
        }

        float delay = villager.PlayDeliverAnim();

        await Task.Delay(TimeSpan.FromSeconds(delay)).ConfigureWithUnrealContext();
        
        if (World.GameMode is not IResourceInterface giveToInterface)
        {
            FinishExecute(false);
            return;
        }
        
        if (takeFromActor is not IResourceInterface takeFromInterface)
        {
            FinishExecute(false);
            return;
        }
        
        takeFromInterface.RemoveResource(out KeyValuePair<EResourceType, int> pair);
        giveToInterface.AddResource(pair);
        FinishExecute(true);
    }
}