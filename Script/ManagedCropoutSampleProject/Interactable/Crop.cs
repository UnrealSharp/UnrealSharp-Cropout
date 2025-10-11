using ManagedCropoutSampleProject.Core.GameMode;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Core;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Interactable;

[UClass]
public partial class ACrop : AResource
{
    public ACrop()
    {
        CooldownTime = 3.0f;
        RequireBuild = false;
    }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public partial float CooldownTime { get; set; }

    public override void SetProgressionState(float state)
    {
        base.SetProgressionState(state);
        SetReady();
    }

    protected override void ConstructionScript_Implementation()
    {
        base.ConstructionScript_Implementation();
        
        List<FName> tags =
        [
            "Farming",
            "Ready"
        ];
        
        Tags.Clear();
        for (int i = 0; i < tags.Count; i++)
        {
            Tags.Insert(i, tags[i]);
        }
    }

    public override float Interact()
    {
        base.Interact();
        GetFarmingProgress(out float delay, out int stage);
        return delay;
    }

    public void GetFarmingProgress(out float delay, out int stage)
    {
        Tags.RemoveAt(1);
        SystemLibrary.SetTimer(SwitchStates, CollectionTime, false);

        int newProgress = (int) float.Truncate(ProgressionState);
        ProgressionState = newProgress >= MeshList.Count - 1 ? 0 : newProgress + 1;

        stage = (int) ProgressionState;
        delay = CollectionTime;
    }

    [UFunction]
    private void SwitchStates()
    {
        if ((int) float.Floor(ProgressionState) == 0)
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
        bool isReadyToHarvest = MeshList.Count - 1 == (int) float.Floor(ProgressionState);
        FName tag = isReadyToHarvest ? "Harvest" : "Ready";

        if (Tags.Count == 1)
        {
            Tags.Add(tag);
        }
        else
        {
            Tags[1] = tag;
        }
        
        int meshIndex = MathLibrary.Truncate(ProgressionState);
        Mesh.SetStaticMesh(MeshList[meshIndex]);
        Mesh.RelativeScale3D = FVector.One;

        UCropoutGameInstance gameInstance = World.GameInstanceAs<UCropoutGameInstance>();
        gameInstance.UpdateAllInteractables();
        PopFarmPlot();
    }

    async void PopFarmPlot()
    {
        UInteractableSettings settings = GetDefault<UInteractableSettings>();
        UCurveFloat curveFloat = await settings.CropPopCurve.LoadAsync();
        
        TDelegate<OnTimelineFloat> onReceiveTimelineValue = new TDelegate<OnTimelineFloat>();
        onReceiveTimelineValue.BindUFunction(this, nameof(OnPopTimelineFloat));
        
        Timeline.AddInterpFloat(curveFloat, onReceiveTimelineValue);
        Timeline.Looping = false;
        Timeline.PlayFromStart();
    }
    
    [UFunction]
    void OnPopTimelineFloat(float value)
    {
        FVector relativeScale = new FVector(1, 1, value);
        Mesh.RelativeScale3D = relativeScale;
    }
}