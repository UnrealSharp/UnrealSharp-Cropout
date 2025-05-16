using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace IslandGen;

[UStruct]
public struct FSpawnerData
{
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public TSoftClassPtr<AActor> ActorToSpawn;

    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public float BiomeScale;

    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public int BiomeCount;
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public int SpawnPerBiome;
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public float RandomRotationRange;
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public float ScaleRange;
}

[UClass]
public class ASpawner : AActor
{
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public TArray<FSpawnerData> SpawnerData { get; set; }
    
    private bool hasLoaded = false;

    protected override void BeginPlay()
    {
        StartLoadingActors();
        base.BeginPlay();
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
            hasLoaded = true;
        }
        catch (Exception e)
        {
            LogIslandGen.Log(e.ToString());
        }
    }
}