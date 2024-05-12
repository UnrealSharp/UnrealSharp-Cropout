using ManagedCropoutSampleProject.Core.Save;
using ManagedCropoutSampleProject.Villagers;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject;

[UClass]
public class CropoutVillager : Pawn, IVillager
{
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public Actor? Target { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public Name CurrentJob { get; set; }
    
    // This is where villagers are assigned new jobs.
    // The Event Action is sent from the player and passes along the target for the villager.
    // Read the Tag from the target and try to change jobs based on that tag name.
    public void Action(Actor? actor)
    {
        if (actor == null || !actor.IsValid || actor.Tags.Count == 0)
        {
            return;
        }
        
        ChangeJob(actor.Tags[0]);

        IGameInstance gameInstance = (IGameInstance) GameplayStatics.GetGameInstance(this);
        gameInstance.UpdateAllVillagers();
    }

    public void ChangeJob(Name newJob)
    {
        CurrentJob = newJob;
    }
}