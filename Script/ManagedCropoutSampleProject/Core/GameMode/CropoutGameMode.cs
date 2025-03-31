using ManagedCropoutSampleProject.Interactable;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Core.GameMode;

[UClass]
public class ACropoutGameMode : AGameModeBase, IResourceInterface
{
    public a
    protected override void BeginPlay()
    {
        
        base.BeginPlay();
    }

    public void RemoveResource(KeyValuePair<EResourceType, int> resource)
    {
        throw new NotImplementedException();
    }

    public void AddResource(KeyValuePair<EResourceType, int> resource)
    {
        throw new NotImplementedException();
    }

    public IDictionary<EResourceType, int> GetCurrentResources()
    {
        throw new NotImplementedException();
    }

    public void RemoveTargetResource(KeyValuePair<EResourceType, int> resource)
    {
        throw new NotImplementedException();
    }

    public EResourceType CheckResource(bool isTarget, int amount)
    {
        throw new NotImplementedException();
    }
}