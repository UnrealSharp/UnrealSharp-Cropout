using ManagedCropoutSampleProject.Interactable;
using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.Core;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI.Tasks;

[UClass]
public partial class UInitialCollectResource : UCropoutBaseTask
{
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector Key_ResourceClass { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector Key_CollectionClass { get; set; }
    
    [UProperty]
    public partial FBlackboardKeySelector Key_Resource { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector Key_ResourceTag { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector Key_TownHall { get; set; }

    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial TSubclassOf<AInteractable> TownHallClass { get; set; }

    public override void ReceiveExecuteAI(AAIController ownerController, APawn controlledPawn)
    {
        if (controlledPawn.Tags.Count == 0) 
        {
            FinishExecute(false);
            return;
        }
        
        FName firstPawnTag = controlledPawn.Tags[0];

        IList<AResource> resources;
        UGameplayStatics.GetAllActorsOfClassWithTag<AResource>(firstPawnTag, out resources);

        if (resources.Count == 0)
        {
            FinishExecute(false);
            return;
        }
        
        UBTFunctionLibrary.SetBlackboardValueAsName(this, Key_ResourceTag, firstPawnTag);
        UBTFunctionLibrary.SetBlackboardValueAsClass(this, Key_ResourceClass, typeof(AResource));

        if (controlledPawn is ACropoutVillager villager)
        {
            UBTFunctionLibrary.SetBlackboardValueAsObject(this, Key_Resource, villager.Target);
        }
        
        AInteractable townHall = UGameplayStatics.GetActorOfClass(TownHallClass);
        UBTFunctionLibrary.SetBlackboardValueAsObject(this, Key_TownHall, townHall);
        
        FinishExecute(true);
    }
}