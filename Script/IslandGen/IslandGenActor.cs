using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.GeometryFramework;
using UnrealSharp.GeometryScriptingCore;

namespace IslandGen;

public delegate void OnIslandGenComplete();

[UClass]
public class AIslandGenActor : ADynamicMeshActor
{
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public FRandomStream Seed { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public float MaxSpawnDistance { get; set; } = 1000.0f;
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public int Islands { get; set; } = 15;
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    private UDynamicMesh DynamicMesh { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public bool PreConstruct { get; set; } = false;
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    private IList<FVector> SpawnPoints { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    private float Radius { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public FVector2D IslandSize { get; set; } = new(800.0f, 5000.0f);
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    UMaterialParameterCollection LandscapeParameterCollection { get; set; }

    private OnIslandGenComplete? _onIslandGenCompleteDelegate;
    private bool _islandGenComplete = false;
    
    public void InitializeFromSeed(FRandomStream seed)
    {
        Seed = seed;
        CreateIsland(true);
    }

    public void BindOrExecute(OnIslandGenComplete onIslandGenComplete)
    {
        if (!_islandGenComplete)
        {
            _onIslandGenCompleteDelegate += onIslandGenComplete;
            return;
        }
        
        onIslandGenComplete();
    }
    
    private void CreateIsland(bool spawnMarkers)
    {
        DynamicMesh = DynamicMeshComponent.DynamicMesh;
        DynamicMesh.Reset();
        SpawnPoints.Clear();
        
        SpawnIslands(spawnMarkers); 
        SetGrassColour();
        
        _islandGenComplete = true;
        _onIslandGenCompleteDelegate?.Invoke();
    }

    private void SpawnIslands(bool spawnMarkers)
    {
        for (int i = 0; i < Islands; i++)
        {
            Radius = MathLibrary.RandomFloatInRangeFromStream(Seed, IslandSize.X.ToFloat(), IslandSize.Y.ToFloat());

            FVector spawnPoint = MathLibrary.RandomUnitVectorFromStream(Seed) * MaxSpawnDistance / 2;
            SpawnPoints.Add(spawnPoint);
        
            FTransform transform = new FTransform();
        
            transform.Location = new FVector(spawnPoint.X, spawnPoint.Y, -800.0f);

            UGeometryScriptLibrary_MeshPrimitiveFunctions.AppendCone(DynamicMesh, new FGeometryScriptPrimitiveOptions(),
                transform, Radius, Radius / 4, 1300, 32, 1, true, EGeometryScriptPrimitiveOriginMode.Base);

            if (!spawnMarkers)
            {
                return;
            }
        
            FTransform markerTransform = new FTransform();
            markerTransform.Location = spawnPoint;
            SpawnActor<ASpawnMarker>(typeof(ASpawnMarker), markerTransform);
        }
        
        FTransform boxTransform = new FTransform();
        boxTransform.Location = new FVector(0.0f, 0.0f, -800.0f);
        float spawnDistance = MaxSpawnDistance * 10000;
        UDynamicMesh mesh = UGeometryScriptLibrary_MeshPrimitiveFunctions.AppendBox(DynamicMesh, new FGeometryScriptPrimitiveOptions(),
            boxTransform, spawnDistance, spawnDistance, 400, 0, 0, 0, EGeometryScriptPrimitiveOriginMode.Base);
        
        
        FGeometryScriptSolidifyOptions solidifyOptions = new FGeometryScriptSolidifyOptions();
        
        FGeometryScript3DGridParameters gridParameters = new FGeometryScript3DGridParameters();
        gridParameters.SizeMethod = EGeometryScriptGridSizingMethod.GridResolution;
        gridParameters.GridCellSize = 0.25f;
        gridParameters.GridResolution = PlatformSwitch(60, 50); 
        
        solidifyOptions.GridParameters = gridParameters;
        solidifyOptions.WindingThreshold = 0.5f;
        solidifyOptions.SolidAtBoundaries = false;
        solidifyOptions.ExtendBounds = 0.0f;
        solidifyOptions.SurfaceSearchSteps = 64;
        solidifyOptions.ThickenShells = false;
        solidifyOptions.ShellThickness = 1.0f;
        
        mesh = UGeometryScriptLibrary_MeshVoxelFunctions.ApplyMeshSolidify(mesh, solidifyOptions);
        mesh = UGeometryScriptLibrary_MeshNormalsFunctions.SetMeshToFacetNormals(mesh);
        
        FGeometryScriptIterativeMeshSmoothingOptions smoothingOptions = new FGeometryScriptIterativeMeshSmoothingOptions();
        smoothingOptions.NumIterations = 6;
        smoothingOptions.Alpha = 0.2f;
        smoothingOptions.EmptyBehavior = EGeometryScriptEmptySelectionBehavior.FullMeshSelection;

        mesh = UGeometryScriptLibrary_MeshDeformFunctions.ApplyIterativeSmoothingToMesh(mesh, new FGeometryScriptMeshSelection(), smoothingOptions);

        mesh = UGeometryScriptLibrary_MeshSubdivideFunctions.ApplyPNTessellation(mesh,
            new FGeometryScriptPNTessellateOptions(), PlatformSwitch(0, 2));
        
        FTransform cutFrame = new FTransform();
        cutFrame.Location = new FVector(0.0f, 0.0f, -390.0f);
        cutFrame.Rotation = new FRotator(180.0f, 0.0f, 0.0f);
        
        FGeometryScriptMeshPlaneCutOptions cutOptions = new FGeometryScriptMeshPlaneCutOptions();
        cutOptions.FillHoles = false;
        cutOptions.HoleFillMaterialID = -1;
        cutOptions.FillSpans = false;
        cutOptions.FlipCutSide = false;
        cutOptions.UVWorldDimension = 1;
        
        mesh = UGeometryScriptLibrary_MeshBooleanFunctions.ApplyMeshPlaneCut(mesh, cutFrame, cutOptions);
        
        mesh = UGeometryScriptLibrary_MeshBooleanFunctions.ApplyMeshPlaneCut(mesh, FTransform.Identity, new FGeometryScriptMeshPlaneCutOptions());
        
        FTransform planeTransform = new FTransform();
        planeTransform.Scale = new FVector(100.0f);

        mesh = UGeometryScriptLibrary_MeshUVFunctions.SetMeshUVsFromPlanarProjection(mesh, 0, planeTransform,
            new FGeometryScriptMeshSelection());
        
        ReleaseAllComputeMeshes();
        AddActorWorldOffset(new FVector(0.0f, 0.0f, -0.05f), false, out _, false);
    }

    private void SetGrassColour()
    {
        FLinearColor color = MaterialLibrary.GetVectorParameterValue(LandscapeParameterCollection, "GrassColour");
        
        MathLibrary.RGBIntoHSV(color, out FLinearColor hsv);

        float randomHue = MathLibrary.RandomFloatInRangeFromStream(Seed, 0.0f, 90.0f) + 103.0f;
        FLinearColor grassColour = MathLibrary.MakeColor(randomHue, hsv.G, hsv.B, hsv.A);
        
        MaterialLibrary.SetVectorParameterValue(LandscapeParameterCollection, "GrassColour", grassColour);
    }

    private int PlatformSwitch(int lowEnd, int highEnd)
    {
        string platform = UGameplayStatics.PlatformName;

        if (platform == "Android" || platform == "IOS" || platform == "Switch")
        {
            return lowEnd;
        }
        
        return highEnd;
    }
}