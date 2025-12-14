using ManagedCropoutSampleProject.Core.GameMode;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Interactable;

[UClass]
public partial class AResource : AInteractable, IResourceInterface
{
    public AResource()
    {
        ResourceAmount = 100;
        CollectionTime = 3;
        CollectionValue = 10;
    }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Resource Properties")]
    public partial EResourceType ResourceType { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Resource Properties")]
    protected partial int ResourceAmount { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Resource Properties")]
    protected partial float CollectionTime { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Resource Properties")]
    protected partial int CollectionValue { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Resource Visuals")]
    protected partial bool UseRandomMesh { get; set; }

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
        
    }

    public void AddResource(KeyValuePair<EResourceType, int> resource)
    {
        
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
        StopWobble();
        
        if (ResourceAmount != -1)
        {
            ResourceAmount = Math.Max(ResourceAmount - CollectionValue, 0);
        
            if (ResourceAmount == 0)
            {
                Death();
            }   
        }
        
        resource = new KeyValuePair<EResourceType, int>(ResourceType, CollectionValue);
    }
}