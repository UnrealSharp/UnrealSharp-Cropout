using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Interactable;

[UClass]
public class AInteractable : AActor
{
    private bool enableGroundBlend = true;

    [UProperty(PropertyFlags.EditAnywhere)]
    public TArray<UStaticMesh> MeshList { get; set; }
    
    [UProperty(DefaultComponent = true, RootComponent = true)]
    public USceneComponent Scene { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = "Scene")]
    public UStaticMeshComponent Mesh { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = "Scene")]
    public UBoxComponent Box { get; set; }
    
    private float _progressionState;
    public bool RequireBuild;

    public void PlacementMode()
    {
        enableGroundBlend = false;
        Mesh.SetStaticMesh(MeshList[0]);
        Tags.Add("PlacementMode");
    }

    public void SetProgressionState(float progression)
    {
        _progressionState = progression;

        if (RequireBuild)
        {
            return;
        }
        
        Tags.Add("Build");
        UStaticMesh newMesh = MeshList[MathLibrary.Floor(_progressionState)];
        Mesh.SetStaticMesh(newMesh);
    }
}