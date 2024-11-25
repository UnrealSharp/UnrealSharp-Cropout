using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Interactable;

[UClass(ClassFlags.Abstract)]
public class AInteractable : AActor
{
    /// Components
    [UProperty(DefaultComponent = true, RootComponent = true)]
    public USceneComponent Root { get; set; }

    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(Root))]
    public UStaticMeshComponent Mesh { get; set; }

    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(Root))]
    public UBoxComponent Box { get; set; }

    /// Public Vars
    [UProperty(PropertyFlags.EditAnywhere | PropertyFlags.BlueprintReadWrite, Category = "Ground Blend")]
    public bool bEnableGroundBlend { get; set; } = true;

    [UProperty(PropertyFlags.EditAnywhere | PropertyFlags.BlueprintReadWrite, Category = "Ground Blend")]
    public UTextureRenderTarget2D RtDraw { get; set; }

    [UProperty(PropertyFlags.EditAnywhere | PropertyFlags.BlueprintReadWrite, Category = "Ground Blend")]
    public float OutlineDraw { get; set; }

    [UProperty(PropertyFlags.EditAnywhere | PropertyFlags.BlueprintReadWrite, Category = "Progression")]
    public bool bRequireBuild { get; set; }

    [UProperty(PropertyFlags.EditAnywhere | PropertyFlags.BlueprintReadWrite, Category = "Progression")]
    public float ProgressionState { get; set; }

    [UProperty(PropertyFlags.EditAnywhere | PropertyFlags.BlueprintReadWrite, Category = "Progression")]
    public TArray<UStaticMesh> MeshList { get; set; }

    [UProperty(PropertyFlags.EditAnywhere | PropertyFlags.BlueprintReadWrite, Category = "Spawn Info")]
    public float BoundGap { get; set; }

    [UProperty(PropertyFlags.EditAnywhere | PropertyFlags.BlueprintReadWrite)]
    public UMaterial ShapeDrawMaterial { get; set; }

    [UFunction]
    public virtual void Interact()
    {
    }

    public void SetProgressionsState(float progression)
    {
        // Set progression state
        ProgressionState = progression;

        // If this interactable requires building before use, apply the Build tag and set starting mesh from mesh list
        if (!bRequireBuild) return;

        Tags.Add("Build");

        var idx = MathLibrary.Floor(ProgressionState);
        if (MeshList.Count <= idx) return;

        var mesh = MeshList[idx];

        if (SystemLibrary.IsValid(mesh))
        {
            Mesh.SetStaticMesh(mesh);
        }
    }

    private void PlacementMode()
    {
        bEnableGroundBlend = false;
        Mesh.SetStaticMesh(MeshList[0]);
        Tags.Clear();
        Tags.Add("PlacementMode");
    }

    protected override void BeginPlay()
    {
        base.BeginPlay();
        SystemLibrary.SetTimerForNextTick(this, nameof(DelayedBeginPlay));
    }

    public override void ConstructionScript()
    {
        const int step = 100;

        base.ConstructionScript();
        Mesh.GetLocalBounds(out _, out var max);

        var steppedX = MathLibrary.Round(max.X) * step;
        var steppedY = MathLibrary.Round(max.Y) * step;
        var steppedZ = MathLibrary.Round(max.Z) * step;

        var xy = Math.Max(100, Math.Max(steppedX, steppedY));
        var vec = new FVector(xy, xy, Math.Max(100, steppedZ));
        var boxExtent = vec + new FVector(BoundGap, BoundGap, BoundGap) * 100.0;

        Box.SetBoxExtent(boxExtent);
        Box.SetWorldRotation(MathLibrary.MakeRotFromX(new FVector(1, 0, 0)), false, out _, false);
    }

    private void DelayedBeginPlay()
    {
        if (bEnableGroundBlend)
        {
            RenderingLibrary.BeginDrawCanvasToRenderTarget(this, RtDraw, out var canvas, out var size, out var context);
            TransformToTexture(size, out var screenPosition, out var screenSize);
            canvas.DrawMaterial(ShapeDrawMaterial, screenPosition, screenSize, new FVector2D(0, 0), new FVector2D(1, 1),
                0, new FVector2D(0.5, 0.5));
            RenderingLibrary.EndDrawCanvasToRenderTarget(this, context);
        }

        GetOverlappingActors(out var overlappingActors, typeof(AInteractable));
        foreach (var overlappingActor in overlappingActors)
        {
            if (FVector.Distance(ActorLocation, overlappingActor.ActorLocation) <= 5 &&
                !overlappingActor.ActorHasTag("PlacementMode"))
            {
                DestroyActor();
            }
        }
    }

    private void TransformToTexture(FVector2D inVector, out FVector2D screenPosition, out FVector2D screenSize)
    {
        GetActorBounds(false, out _, out var boxExtent, false);
        var size = Math.Min(boxExtent.X, boxExtent.Y) / 10000 * inVector.X * OutlineDraw;
        screenSize = new FVector2D(size, size);
        var screenPositionValue =
            ActorLocation / 10000 / 20000 * inVector.X - new FVector(size / 2, size / 2, size / 2);
        screenPosition = new FVector2D(screenPositionValue.X, screenPositionValue.Y);
    }
}