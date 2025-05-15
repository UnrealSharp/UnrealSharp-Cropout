using ManagedCropoutSampleProject.Interactable;

namespace ManagedCropoutSampleProject.Core.GameMode;

public interface IResourceInterface
{
    public void RemoveTargetResource(KeyValuePair<EResourceType, int> resource);
    public void AddResource(KeyValuePair<EResourceType, int> resource);
    public IDictionary<EResourceType, int> GetCurrentResources();
    public bool CheckResource(EResourceType resourceType, out int amount);
    public void RemoveResource(out KeyValuePair<EResourceType, int> resource);
}