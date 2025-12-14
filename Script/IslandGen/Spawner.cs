using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.NavigationSystem;

namespace IslandGen;

public delegate void FOnSpawningCompleted();

[UStruct]
public partial struct FSpawnerData
{
    [UProperty(PropertyFlags.EditAnywhere)]
    public TSoftClassPtr<AActor> ActorToSpawn;

    [UProperty(PropertyFlags.EditAnywhere)]
    public float BiomeScale;

    [UProperty(PropertyFlags.EditAnywhere)]
    public int BiomeCount;
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public int SpawnPerBiome;
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public float RandomRotationRange;
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public float ScaleRange;
}

[UStruct]
public partial struct FSpawnInstancesData
{
    [UProperty(PropertyFlags.EditAnywhere)]
    public UStaticMesh StaticMesh;

    [UProperty(PropertyFlags.EditAnywhere)]
    public float BiomeScale;

    [UProperty(PropertyFlags.EditAnywhere)]
    public int BiomeCount;

    [UProperty(PropertyFlags.EditAnywhere)]
    public int SpawnPerBiome;
}

[UClass]
public partial class ASpawner : AActor
{
    public ASpawner()
    {
        AutoSpawn = true;
        ActorSwitch = true;
    }
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public partial TArray<FSpawnerData> SpawnerData { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial TArray<FSpawnInstancesData> SpawnInstances { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial bool AutoSpawn { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public partial bool ActorSwitch { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial ANavigationData NavigationData { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial FRandomStream Seed { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public partial bool CallSave { get; set; }
    
    private bool _hasLoaded;
    private int _indexCounter;
    private FTimerHandle _timerHandle;

    public FOnSpawningCompleted? spawningCompleted;

    public override void BeginPlay()
    {
        base.BeginPlay();
        
        StartLoadingActors();
    }

    public async void StartLoadingActors()
    {
        try
        {
            List<FSoftObjectPath> softObjectPaths = new(SpawnerData.Count);
            foreach (FSpawnerData data in SpawnerData)
            {
                if (!data.ActorToSpawn.IsNull)
                {
                    softObjectPaths.Add(data.ActorToSpawn.SoftObjectPath);
                }
            }

            await softObjectPaths.LoadAsync();
            _hasLoaded = true;
        }
        catch (Exception e)
        {
            LogIslandGen.Log(e.ToString());
        }
    }

    public void SpawnMeshOnly()
    {
        ActorSwitch = false;
        StartWaitSpawn();
    }

    public void SpawnRandom()
    {
        StartWaitSpawn();
    }

    private async void StartWaitSpawn()
    {
        await Task.Delay(5).ConfigureWithUnrealContext();
        _indexCounter = 0;
       _timerHandle = SystemLibrary.SetTimer(IsReadyToSpawn, 0.5f, true, -0.5f);
    }
    
    [UFunction]
    private void IsReadyToSpawn()
    {
        if (UNavigationSystemV1.IsNavigationBeingBuilt() || !_hasLoaded)
        {
            return;
        }
        
        SystemLibrary.PauseTimerHandle(_timerHandle);

        if (ActorSwitch)
        {
            FSpawnerData data = SpawnerData[_indexCounter];
            SpawnAssets(data.ActorToSpawn.Class, data);

            _indexCounter++;

            if (_indexCounter >= SpawnerData.Count)
            {
                _indexCounter = 0;
                ActorSwitch = false;
            }
            
            SystemLibrary.UnPauseTimerHandle(_timerHandle);
        }
        else
        {
            FSpawnInstancesData instanceData = SpawnInstances[_indexCounter];
            UInstancedStaticMeshComponent newMeshComponent = AddComponentByClass<UInstancedStaticMeshComponent>( false, FTransform.Identity);
            newMeshComponent.SetStaticMesh(instanceData.StaticMesh);
            
            SpawnInstance(newMeshComponent, instanceData.BiomeScale, instanceData.BiomeCount, instanceData.SpawnPerBiome);
            _indexCounter++;
            
            if (_indexCounter >= SpawnInstances.Count)
            {
                if (CallSave)
                {
                    SpawningComplete();
                }
            }
            else
            {
                SystemLibrary.UnPauseTimerHandle(_timerHandle);
            }
        }
    }

    private void SpawnAssets(TSubclassOf<AActor> actor, FSpawnerData data)
    {
        for (int i = 0; i < data.BiomeCount; i++)
        {
            UNavigationSystemV1.GetRandomLocationInNavigableRadius(FVector.Zero, out FVector vector, 10000.0f,
                NavigationData);

            int spawns = MathLibrary.RandomIntInRange(Seed, 1, data.SpawnPerBiome);
            for (int j = 0; j < spawns; j++)
            {
                UNavigationSystemV1.GetRandomLocationInNavigableRadius(vector, out var randomLocation, data.BiomeScale,
                    NavigationData);

                FTransform spawnTransform = new FTransform();
                spawnTransform.Location = randomLocation;

                double yaw = MathLibrary.RandomFloatInRange(0.0f, data.RandomRotationRange);
                spawnTransform.Rotation = new FRotator(0.0f, yaw, 0.0f);

                double scaleXYZ = MathLibrary.RandomFloatInRange(1, data.ScaleRange + 1);
                spawnTransform.Scale = new FVector(scaleXYZ);

                SpawnActor(actor, spawnTransform);
            }
        }
    }

    private void SpawnInstance(UInstancedStaticMeshComponent meshComponent, float radius, int biomeCount, int maxSpawn)
    {
        for (int i = 0; i < biomeCount; i++)
        {
            UNavigationSystemV1.GetRandomLocationInNavigableRadius(FVector.Zero, out FVector randomLocation, 10000,
                NavigationData);

            int spawnCount = MathLibrary.RandomIntInRange(Seed, 0, maxSpawn);
            for (int j = 0; j < spawnCount; j++)
            {
                UNavigationSystemV1.GetRandomLocationInNavigableRadius(randomLocation, out FVector spawnPoint, radius,
                    NavigationData);

                FTransform instanceTransform = new FTransform();
                instanceTransform.Location = new FVector(spawnPoint.X, spawnPoint.Y, 0.0f);
                instanceTransform.Rotation = FRotator.ZeroRotator;

                double length = MathLibrary.VSize(randomLocation - spawnPoint);
                double alpha = length / radius;
                double scale = double.Lerp(0.8, 1.5, alpha);

                instanceTransform.Scale = new FVector(scale);
                meshComponent.AddInstance(instanceTransform, true);
            }
        }
    }

    private FVector SteppedPosition(FVector vector)
    {
        FVector steppedPosition = vector / 200.0f;
        return new FVector(double.Round(steppedPosition.X), double.Round(steppedPosition.Y), 0.0f) * 200.0f;
    }

    private void SpawningComplete()
    {
        if (spawningCompleted == null)
        {
            return;
        }
        
        spawningCompleted();
    }
}