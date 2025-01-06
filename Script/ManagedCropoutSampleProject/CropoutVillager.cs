using ManagedCropoutSampleProject.Core.Save;
using ManagedCropoutSampleProject.Villagers;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.UnrealSharpCore;

namespace ManagedCropoutSampleProject;

[UClass]
public class ACropoutVillager : APawn, IVillager
{
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public AActor? Target { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public FName CurrentJob { get; set; }
    
    // This is where villagers are assigned new jobs.
    // The Event Action is sent from the player and passes along the target for the villager.
    // Read the Tag from the target and try to change jobs based on that tag name.
    public void Action(AActor? actor)
    {
        if (actor == null || !actor.IsValid || actor.Tags.Count == 0)
        {
            return;
        }
        
        ChangeJob(actor.Tags[0]);

        IGameInstance gameInstance = (IGameInstance) UGameplayStatics.GameInstance;
        gameInstance.UpdateAllVillagers();
    }

    public void ChangeJob(FName newJob)
    {
        CurrentJob = newJob;
    }
}