using ManagedCropoutSampleProject.Core.GameMode;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Interactable;

[UClass]
public class AResource : AInteractable, IResourceInterface
{
    public AResource()
    {
        ResourceAmount = 100;
        CollectionTime = 3;
        CollectionValue = 10;
    }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Resource Properties")]
    public EResourceType ResourceType { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Resource Properties")]
    protected int ResourceAmount { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Resource Properties")]
    protected float CollectionTime { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Resource Properties")]
    protected int CollectionValue { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Resource Visuals")]
    protected bool UseRandomMesh { get; set; }

    public override void ConstructionScript()
    {
        base.ConstructionScript();
        
        Tags.Add(ResourceType.ToString());
        
        if (UseRandomMesh)
        {
            Mesh.SetStaticMesh(MeshList[MathLibrary.RandomIntegerInRange(0, MeshList.Count - 1)]);
        }
    }

    public void Death()
    {
        DestroyActor();
    }

    public override float Interact()
    {
        return CollectionTime;
    }

    public void RemoveTargetResource(KeyValuePair<EResourceType, int> resource)
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

    public bool CheckResource(EResourceType resourceType, out int amount)
    {
        throw new NotImplementedException();
    }

    public void RemoveResource(out KeyValuePair<EResourceType, int> resource)
    {
        if (ResourceAmount != -1)
        {
            ResourceAmount = Math.Max(ResourceAmount - CollectionValue, 0);
        
            if (ResourceAmount == 0)
            {
                Death();
            }
        }
        
        resource = new KeyValuePair<EResourceType, int>(ResourceType, ResourceAmount);
    }
}