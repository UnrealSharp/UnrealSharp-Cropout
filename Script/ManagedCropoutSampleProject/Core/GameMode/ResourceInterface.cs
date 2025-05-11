using ManagedCropoutSampleProject.Interactable;

namespace ManagedCropoutSampleProject.Core.GameMode;

public interface IResourceInterface
{
    public void RemoveResource(KeyValuePair<EResourceType, int> resource);
    public void AddResource(KeyValuePair<EResourceType, int> resource);
    public IDictionary<EResourceType, int> GetCurrentResources();
    public bool CheckResource(EResourceType resourceType, out int amount);
}