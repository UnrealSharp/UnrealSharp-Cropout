using System.ComponentModel;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.GeometryFramework;
using UnrealSharp.GeometryScriptingCore;

namespace IslandGen;

public delegate void OnIslandGenComplete();

[UClass]
public partial class AIslandGenActor : ADynamicMeshActor
{
    public AIslandGenActor()
    {
        Islands = 17;
        IslandSize = new(800.0f, 5000.0f);
    }
    
    [UProperty(PropertyFlags.EditAnywhere), Category("Island Generation")]
    public partial FRandomStream Seed { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere), Category("Island Generation")]
    public partial float MaxSpawnDistance { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere | PropertyFlags.BlueprintReadOnly), Category("Island Generation")]
    public partial int Islands { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere), Category("Island Generation")]
    private partial UDynamicMesh DynamicMesh { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere), Category("Island Generation")]
    private partial IList<FVector> SpawnPoints { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere), Category("Island Generation")]
    private partial float Radius { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere), Category("Island Generation")]
    public partial FVector2D IslandSize { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    private partial UMaterialParameterCollection LandscapeParameterCollection { get; set; }

    private OnIslandGenComplete? _onIslandGenCompleteDelegate;
    private bool _islandGenComplete = false;

    public void InitializeFromSeed(FRandomStream seed)
    {
        Seed = seed;
        CreateIsland(true);
    }

    [UFunction(CallInEditor = true), Category("Island Generation")]
    public void GenerateIsland()
    {
        CreateIsland(false);
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
        
        _islandGenComplete = true;
        _onIslandGenCompleteDelegate?.Invoke();
    }

    private void SpawnIslands(bool spawnMarkers)
    {
        FRandomStream seed = Seed;
        for (int i = 0; i < Islands; i++)
        {
            Radius = MathLibrary.RandomFloatInRangeFromStream(seed, IslandSize.X.ToFloat(), IslandSize.Y.ToFloat());
            seed.GetFraction();

            FVector spawnPoint = MathLibrary.RandomUnitVectorFromStream(seed) * MaxSpawnDistance / 2;
            seed.GetFraction();
            
            spawnPoint.Z = 0.0f;
            
            SpawnPoints.Add(spawnPoint);
        
            FTransform transform = new FTransform();
            transform.Location = new FVector(spawnPoint.X, spawnPoint.Y, -800.0f);
            transform.Scale = FVector.One;

            FGeometryScriptPrimitiveOptions options = new FGeometryScriptPrimitiveOptions
            {
                PolyGroupMode = EGeometryScriptPrimitivePolygroupMode.PerFace,
                FlipOrientation = false,
                UVMode = EGeometryScriptPrimitiveUVMode.Uniform,
                MaterialID = 0,
            };

            UGeometryScriptLibrary_MeshPrimitiveFunctions.AppendCone(DynamicMesh, options,
                transform, Radius, Radius / 4, 1300, 32, 1, true, EGeometryScriptPrimitiveOriginMode.Base);

            if (!spawnMarkers)
            {
                continue;
            }
        
            FTransform markerTransform = new FTransform();
            markerTransform.Location = spawnPoint;
            markerTransform.Scale = FVector.One;
            
            SpawnActor<ASpawnMarker>(typeof(ASpawnMarker), markerTransform);
        }
        
        FTransform boxTransform = new FTransform();
        boxTransform.Location = new FVector(0.0f, 0.0f, -800.0f);
        boxTransform.Scale = FVector.One;
        
        float spawnDistance = MaxSpawnDistance + 10000;
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
        solidifyOptions.ExtendMeshBounds = 0.0f;
        solidifyOptions.SurfaceSearchSteps = 64;
        solidifyOptions.ThickenShells = false;
        solidifyOptions.ShellThickness = 1.0f;
        
        mesh = UGeometryScriptLibrary_MeshVoxelFunctions.ApplyMeshSolidify(mesh, solidifyOptions);
        mesh = UGeometryScriptLibrary_MeshNormalsFunctions.SetPerVertexNormals(mesh);
        
        FGeometryScriptIterativeMeshSmoothingOptions smoothingOptions = new FGeometryScriptIterativeMeshSmoothingOptions();
        smoothingOptions.NumIterations = 6;
        smoothingOptions.Alpha = 0.2f;
        smoothingOptions.EmptyBehavior = EGeometryScriptEmptySelectionBehavior.FullMeshSelection;

        mesh = UGeometryScriptLibrary_MeshDeformFunctions.ApplyIterativeSmoothingToMesh(mesh, new FGeometryScriptMeshSelection(), smoothingOptions);

        mesh = UGeometryScriptLibrary_MeshSubdivideFunctions.ApplyPNTessellation(mesh,
            new FGeometryScriptPNTessellateOptions(), PlatformSwitch(0, 2));
        
        FTransform cutFrame = new FTransform();
        cutFrame.Location = new FVector(0.0f, 0.0f, -390.0f);
        cutFrame.Rotation = new FRotator(0.0f, 0.0f, 180.0f);
        cutFrame.Scale = FVector.One;
        
        FGeometryScriptMeshPlaneCutOptions cutOptions = new FGeometryScriptMeshPlaneCutOptions();
        cutOptions.FillHoles = false;
        cutOptions.HoleFillMaterialID = -1;
        cutOptions.FillSpans = false;
        cutOptions.FlipCutSide = false;
        cutOptions.UVWorldDimension = 1;
        
        mesh = UGeometryScriptLibrary_MeshBooleanFunctions.ApplyMeshPlaneCut(mesh, cutFrame, cutOptions);

        cutOptions.FillHoles = true;
        cutOptions.HoleFillMaterialID = -1;
        cutOptions.FillSpans = true;
        cutOptions.FlipCutSide = false;
        cutOptions.UVWorldDimension = 1;
        
        mesh = UGeometryScriptLibrary_MeshBooleanFunctions.ApplyMeshPlaneCut(mesh, FTransform.Identity, cutOptions);
        
        FTransform planeTransform = new FTransform();
        planeTransform.Scale = new FVector(100.0f);

        mesh = UGeometryScriptLibrary_MeshUVFunctions.SetMeshUVsFromPlanarProjection(mesh, 0, planeTransform,
            new FGeometryScriptMeshSelection());
        
        ReleaseAllComputeMeshes();
        AddActorWorldOffset(new FVector(0.0f, 0.0f, 0.05f), false, out _, false);
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