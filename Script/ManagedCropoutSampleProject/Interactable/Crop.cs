using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.Core.Save;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Interactable;

[UClass]
public class ACrop : AResource
{
    public ACrop()
    {
        CooldownTime = 3.0f;
    }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public float CooldownTime { get; set; }
    
    public override float Interact()
    {
        GetFarmingProgress(out float delay, out int stage);
        return delay;
    }

    public void GetFarmingProgress(out float delay, out int stage)
    {
        Tags.RemoveAt(1);
        SystemLibrary.SetTimer(SetReady, CooldownTime, false);

        int newProgress = (int) float.Truncate(ProgressionState) + 1;
        newProgress = MeshList.Count >= newProgress ? newProgress : 0;

        stage = newProgress;
        delay = CollectionTime;
    }

    private void SwitchStates()
    {
        if (float.Floor(ProgressionState) == 0)
        {
            SetReady();
        }
        else
        {
            SystemLibrary.SetTimer(SetReady, CooldownTime, false);
        }
    }

    [UFunction]
    private void SetReady()
    {
        bool IsReadyToHarvest = MeshList.Count == float.Floor(ProgressionState);
        FName tag = IsReadyToHarvest ? "Harvest" : "Ready";
        Tags[1] = tag;
        
        Mesh.SetStaticMesh(MeshList[(int) float.Truncate(ProgressionState)]);

        UCropoutGameInstance gameInstance = World.GameInstanceAs<UCropoutGameInstance>();
        gameInstance.UpdateAllInteractables();
    }
}