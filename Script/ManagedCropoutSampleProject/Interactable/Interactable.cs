using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Interactable;

[UClass]
public class AInteractable : AActor
{
    public AInteractable()
    {
        OutlineDraw = 1.5f;
    }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Meshes")]
    public TArray<UStaticMesh> MeshList { get; set; }
    
    [UProperty(DefaultComponent = true, RootComponent = true)]
    public USceneComponent Scene { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = "Scene")]
    public UStaticMeshComponent Mesh { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = "Scene")]
    public UBoxComponent Box { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public float BoundGap { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Visuals")]
    protected bool EnableGroundBlend { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Visuals")]
    protected UTextureRenderTarget2D RenderTarget { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Visuals")]
    protected UMaterialInterface DrawMaterial { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Visuals")]
    protected float OutlineDraw { get; set; }
    
    private float _progressionState;
    public bool RequireBuild;

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

    protected override void BeginPlay()
    {
        FLatentActionInfo actionInfo = new FLatentActionInfo();
        actionInfo.CallbackTarget = this;
        actionInfo.ExecutionFunction = nameof(SetupInteractable);
        SystemLibrary.DelayUntilNextTick(actionInfo);
        
        base.BeginPlay();
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

    public virtual float Interact()
    {
        return 0.0f;
    }

    public void PlayWobble()
    {
        
    }
    
    public void PlacementMode()
    {
        EnableGroundBlend = false;
        Mesh.SetStaticMesh(MeshList[0]);
        Tags.Add("PlacementMode");
    }

    [UFunction]
    public void SetupInteractable()
    {
        SetupRenderTarget();
        RemoveOverlappingInteractables();
    }

    private void SetupRenderTarget()
    {
        return;
        if (!EnableGroundBlend)
        {
            return;
        }
        
        RenderingLibrary.BeginDrawCanvasToRenderTarget(RenderTarget, out UCanvas canvas, out FVector2D size, out FDrawToRenderTargetContext context);

        FVector cachedActorLocation = ActorLocation;
        cachedActorLocation += 10000;
        cachedActorLocation /= 20000;
        
        GetActorBounds(false, out FVector origin, out FVector boxExtent);
        double minXY = double.Min(origin.Y, origin.Z) / 10000;
        minXY *= size.X * OutlineDraw;

        cachedActorLocation *= minXY;
        cachedActorLocation -= new FVector(minXY / 2);
        FVector2D newOrigin = new FVector2D(cachedActorLocation);
        
        canvas.DrawMaterial(DrawMaterial, newOrigin, new FVector2D(minXY, minXY), new FVector2D());
        RenderingLibrary.EndDrawCanvasToRenderTarget(context);
    }

    private void RemoveOverlappingInteractables()
    {
        GetOverlappingActors<AInteractable>(out IList<AInteractable> actors);
        FVector MyLocation = ActorLocation;
        
        foreach (AInteractable actor in actors)
        {
            FVector overlappingActorLocation = actor.ActorLocation;
            
            if (FVector.Distance(MyLocation, overlappingActorLocation) < 5 && !actor.ActorHasTag("PlacementMode"))
            {
                continue;
            }
            
            actor.DestroyActor();
        }
    }
}