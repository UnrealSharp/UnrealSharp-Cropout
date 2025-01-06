using ManagedCropoutSampleProject.Core.Save;
using ManagedCropoutSampleProject.Interactable;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CommonUI;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Core.GameMode;

[UClass]
public class UCropoutGameInstance : UGameInstance, IGameInstance, IPlayer
{
    public void UpdateAllVillagers()
    {
        
    }

    public void BeginBuild(TSubclassOf<AInteractable> targetClass, IDictionary<EResourceType, int> resourceCost)
    {
        throw new NotImplementedException();
    }

    public void SwitchBuildMode(bool switchBuildMode)
    {
        throw new NotImplementedException();
    }

    public void AddUI(TSubclassOf<UCommonActivatableWidget> widget)
    {
        throw new NotImplementedException();
    }

    public void RemoveCurrentUILayer()
    {
        throw new NotImplementedException();
    }
}