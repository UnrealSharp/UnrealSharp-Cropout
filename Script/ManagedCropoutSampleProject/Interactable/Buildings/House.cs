using System.ComponentModel;
using ManagedCropoutSampleProject.Core.GameMode;
using UnrealSharp.Attributes;

namespace ManagedCropoutSampleProject.Interactable.Buildings;

[UClass]
public partial class AHouse : ABuilding
{
    public AHouse()
    {
        VillagerCapacity = 2;
    }
    
    [UProperty(PropertyFlags.EditDefaultsOnly), Category("House Properties")]
    public partial int VillagerCapacity { get; set; }
    
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