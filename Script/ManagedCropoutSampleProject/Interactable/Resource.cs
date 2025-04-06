using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Interactable;

[UClass]
public class AResource : AInteractable
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
    protected float ResourceAmount { get; set; }
    
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
}