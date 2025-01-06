using ManagedCropoutSampleProject.Interactable;
using UnrealSharp;
using UnrealSharp.CommonUI;

namespace ManagedCropoutSampleProject.Core;

public interface IPlayer
{
    public void BeginBuild(TSubclassOf<AInteractable> targetClass, IDictionary<EResourceType, int> resourceCost);
    public void SwitchBuildMode(bool switchBuildMode);
    public void AddUI(TSubclassOf<UCommonActivatableWidget> widget);
    public void RemoveCurrentUILayer();
}