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
        RequireBuild = false;
        EnableGroundBlend = true;
    }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Meshes")]
    public TArray<UStaticMesh> MeshList { get; set; }
    
    [UProperty(DefaultComponent = true, RootComponent = true)]
    public USceneComponent Scene { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = "Scene")]
    public UStaticMeshComponent Mesh { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = "Scene")]
    public UBoxComponent Box { get; set; }
    
    [UProperty(DefaultComponent = true)]
    public UTimelineComponent Timeline { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public float BoundGap { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Visuals")]
    protected bool EnableGroundBlend { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Visuals")]
    protected float OutlineDraw { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Progression")]
    public bool RequireBuild { get; set; }

    public float ProgressionState;
    
    public virtual void SetProgressionState(float state)
    {
        ProgressionState = state;

        if (!RequireBuild)
        {
            return;
        }

        if (!Tags.Contains("Build"))
        {
            Tags.Add("Build"); 
        }
            
        int meshIndex = MathLibrary.Floor(ProgressionState);
        if (meshIndex >= MeshList.Count - 1)
        {
            return;
        }
            
        UStaticMesh newMesh = MeshList[meshIndex];
        Mesh.SetStaticMesh(newMesh);
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

    public async void PlayWobble(FVector wobbleLocation)
    {
        UInteractableSettings settings = GetDefault<UInteractableSettings>();
        UCurveFloat curveFloat = await settings.WobbleCurve.LoadAsync();

        FVector wobbleVector = wobbleLocation - ActorLocation;
        Mesh.SetVectorParameterValueOnMaterials("Wobble Vector", wobbleVector);
        
        TDelegate<OnTimelineFloat> onReceiveTimelineValue = new TDelegate<OnTimelineFloat>();
        onReceiveTimelineValue.BindUFunction(this, nameof(OnTimelineFloat));
        
        Timeline.AddInterpFloat(curveFloat, onReceiveTimelineValue);
        Timeline.Looping = false;
        Timeline.Play();
    }
    
    public void StopWobble()
    {
        Timeline.Reverse();
    }
    
    [UFunction]
    public void OnTimelineFloat(float value)
    {
        Mesh.SetScalarParameterValueOnMaterials("Wobble", value);
    }
    
    public virtual void PlacementMode()
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

    private async void SetupRenderTarget()
    {
        /*if (!EnableGroundBlend)
        {
            return;
        }

        UInteractableSettings settings = GetDefault<UInteractableSettings>();
        await settings.LoadInteractableSettingsAsync();
        
        if (!settings.IsValid)
        {
            return;
        }
        
        RenderingLibrary.BeginDrawCanvasToRenderTarget(settings.RenderTarget.Object, 
            out UCanvas canvas, 
            out FVector2D size, 
            out FDrawToRenderTargetContext context);
        
        GetActorBounds(false, out FVector origin, out FVector boxExtent);
        double minXY = double.Min(boxExtent.X, boxExtent.Y) / 10000;
        minXY *= size.X * OutlineDraw;
        
        FVector cachedActorLocation = ActorLocation;
        cachedActorLocation += 10000;
        cachedActorLocation /= 20000;
        cachedActorLocation *= size.X;
        
        cachedActorLocation -= new FVector(minXY / 2);
        FVector2D newOrigin = new FVector2D(cachedActorLocation);
        
        canvas.DrawMaterial(settings.DrawMaterial.Object, newOrigin, new FVector2D(minXY, minXY), 
            new FVector2D(), 
            FVector2D.One, 
            0.0f, 
            new FVector2D(0.5f, 0.5f));
        
        RenderingLibrary.EndDrawCanvasToRenderTarget(context);*/
    }

    private void RemoveOverlappingInteractables()
    {
        GetOverlappingActors<AInteractable>(out IList<AInteractable> actors);
        FVector myLocation = ActorLocation;
        
        foreach (AInteractable actor in actors)
        {
            if (FVector.Distance(myLocation, actor.ActorLocation) < 5 && !actor.ActorHasTag("PlacementMode"))
            {
                actor.DestroyActor();
            }
        }
    }
}