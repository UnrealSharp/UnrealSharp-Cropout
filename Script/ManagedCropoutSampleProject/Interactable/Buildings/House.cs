using ManagedCropoutSampleProject.Core.GameMode;
using UnrealSharp.Attributes;

namespace ManagedCropoutSampleProject.Interactable.Buildings;

[UClass]
public class AHouse : ABuilding
{
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "House Properties")]
    public int VillagerCapacity { get; set; } = 2;
    
    private bool _hasSpawnedVillagers = false;
    
    public override void ConstructionComplete()
    {
        base.ConstructionComplete();

        if (_hasSpawnedVillagers)
        {
            return;
        }
        
        _hasSpawnedVillagers = true;
        ACropoutGameMode gameMode = World.GameModeAs<ACropoutGameMode>();
        gameMode.SpawnVillagers(VillagerCapacity);
    }

    public override void PlacementMode()
    {
        base.PlacementMode();
        NavBlocker.DestroyComponent(this);
    }
}