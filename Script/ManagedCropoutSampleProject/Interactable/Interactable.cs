using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
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
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public float BoundGap { get; set; }
    
    private float _progressionState;

    public float ProgressionState
    {
        get => _progressionState;
        set
        {
            _progressionState = value;

            if (RequireBuild)
            {
                return;
            }
        
            Tags.Add("Build");
            UStaticMesh newMesh = MeshList[MathLibrary.Floor(_progressionState)];
            Mesh.SetStaticMesh(newMesh);
        }
    }
    
    public bool RequireBuild;

    public void PlacementMode()
    {
        enableGroundBlend = false;
        Mesh.SetStaticMesh(MeshList[0]);
        Tags.Add("PlacementMode");
    }

    public override void ConstructionScript()
    {
        base.ConstructionScript();
        
        // Creates a collision box around the bounds of the actor rounded to grid spacing. This is what is checked when placing actors to make sure they don't overlap.
        Mesh.GetLocalBounds(out FVector origin, out FVector boxExtent);
        float gridSize = 100.0f;
        FVector newOrigin = boxExtent / 100.0f;
        newOrigin.X = MathLibrary.Round(newOrigin.X / gridSize) * gridSize;
        newOrigin.Y = MathLibrary.Round(newOrigin.Y / gridSize) * gridSize;
        newOrigin.Z = MathLibrary.Round(newOrigin.Z / gridSize) * gridSize;
        
        double XYMax = double.Max(newOrigin.Y, newOrigin.Z);
        double ZMax = double.Max(newOrigin.Z, 100);
        
        FVector newExtent = new FVector(double.Max(XYMax, 100), double.Max(XYMax, 100), double.Max(ZMax, 100));
        newExtent += BoundGap * 100.0f;
        Box.SetBoxExtent(newExtent);
    }
}