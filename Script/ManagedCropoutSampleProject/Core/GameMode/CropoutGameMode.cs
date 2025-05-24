using IslandGen;
using ManagedCropoutSampleProject.Core.Save;
using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.Interactable.Buildings;
using ManagedCropoutSampleProject.UI;
using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.AudioModulation;
using UnrealSharp.CommonUI;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.NavigationSystem;

namespace ManagedCropoutSampleProject.Core.GameMode;

[UMultiDelegate]
public delegate void FOnResourceChanged(EResourceType resourceType, int amount);

[UMultiDelegate]
public delegate void FOnUpdateVillagers(int villagerCount);

[UClass]
public partial class ACropoutGameMode : AGameModeBase, IResourceInterface
{
    [UProperty(PropertyFlags.BlueprintAssignable)]
    public TMulticastDelegate<FOnResourceChanged> OnResourceChanged { get; set; }
    
    [UProperty(PropertyFlags.BlueprintAssignable)]
    public TMulticastDelegate<FOnUpdateVillagers> OnUpdateVillagers { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected TMap<EResourceType, int> Resources { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected TSubclassOf<ULayerGameWidget> LayerGameWidgetClass { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public ULayerGameWidget GameWidget { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public USoundControlBus CropoutPianoBus { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public TSubclassOf<ACropoutVillager> VillagerClass { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public int VillagerCount { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public TSubclassOf<ATownHall> TownHallClass { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected USoundControlBus CropoutMusicBus { get; set; }
    
    private bool musicIsPlaying;
    private bool hasEndGame;
    private ATownHall townHall;
    private ASpawner spawner;
    
    protected override void BeginPlay()
    {
        base.BeginPlay();
        CreateGameHUD();
        
        UCropoutGameInstance gameInstance = World.GameInstanceAs<UCropoutGameInstance>();
        gameInstance.UpdateAllResources(Resources);
        
        InitializeSpawnerReference();

        AIslandGenActor genActor = UGameplayStatics.GetActorOfClass<AIslandGenActor>();
        genActor.InitializeFromSeed(gameInstance.SaveObject.RandomStream);
        genActor.BindOrExecute(OnGenIslandCompleted);
    }

    private void OnGenIslandCompleted()
    {
        UCropoutGameInstance gameInstance = World.GameInstanceAs<UCropoutGameInstance>();

        if (gameInstance.HasSave)
        {
            SpawnLoadedInteractables(gameInstance);
            
            Resources.Clear();
            foreach (KeyValuePair<EResourceType, int> resource in Resources)
            {
                Resources.Add(resource.Key, resource.Value);
            }
            
            spawner.SpawnMeshOnly();
            SpawnLoadedVillagers(gameInstance);
            
            UAudioModulationStatics.SetGlobalControlBusMixValue(CropoutMusicBus, 0.0f, 0.0f);
        }
        else
        {
            BeginSpawning();
            UAudioModulationStatics.SetGlobalControlBusMixValue(CropoutMusicBus, 1.0f, 0.0f);
        }
    }

    private void SpawnLoadedInteractables(UCropoutGameInstance gameInstance)
    {
        List<FInteractableSaveData> interactables = gameInstance.SaveObject.Interactables.ToList();
        foreach (FInteractableSaveData savedInteractable in interactables)
        {
            AInteractable interactable = SpawnActor(savedInteractable.Type, savedInteractable.Transform);
            interactable.RequireBuild = savedInteractable.Tag == "Build";
            interactable.SetProgressionState(savedInteractable.Health);

            if (savedInteractable.Type.IsChildOf(typeof(ATownHall)))
            {
                townHall = (interactable as ATownHall)!;
            }
        }
    }

    private void SpawnLoadedVillagers(UCropoutGameInstance gameInstance)
    {
        List<FVillagerSaveData> interactables = gameInstance.SaveObject.Villagers.ToList();
        foreach (FVillagerSaveData savedVillager in interactables)
        {
            FVector spawnLocation = savedVillager.Transform;
            spawnLocation.Z = 42.0f;
            
            ACropoutVillager villager = (ACropoutVillager) AIHelperLibrary.SpawnAIFromClass(VillagerClass.As<APawn>(), null, spawnLocation, FRotator.ZeroRotator);
            villager.ChangeJob(savedVillager.Task);
        }
        
        OnUpdateVillagers.Invoke(gameInstance.SaveObject.Villagers.Count);
        gameInstance.UpdateAllResources(Resources);
    }

    private void InitializeSpawnerReference()
    {
        spawner = UGameplayStatics.GetActorOfClass<ASpawner>();
    }

    private void BeginSpawning()
    {
        UGameplayStatics.GetAllActorsOfClass<ASpawnMarker>(out var spawnMarkers);

        int randomSpawnMarkerIndex = MathLibrary.RandomIntegerInRange(0, spawnMarkers.Count - 1);
        ASpawnMarker randomSpawnMarker = spawnMarkers[randomSpawnMarkerIndex];
        
        townHall = SpawnActor(TownHallClass, randomSpawnMarker.ActorTransform, ESpawnActorCollisionHandlingMethod.AlwaysSpawn);
        
        for (int i = 0; i < 2; i++)
        {
            SpawnVillager();
        }

        OnUpdateVillagers.Invoke(VillagerCount);
        
        UCropoutGameInstance gameInstance = World.GameInstanceAs<UCropoutGameInstance>();
        gameInstance.UpdateAllVillagers();
        
        spawner.SpawnRandom();
    }

    private void CreateGameHUD()
    {
        GameWidget = CreateWidget(LayerGameWidgetClass);
        GameWidget.AddToViewport();
        GameWidget.ActivateWidget();
    }

    public void EndGame(bool win)
    {
        if (hasEndGame)
        {
            return;
        }
        
        hasEndGame = true;
        GameWidget.EndGame(win);
    }

    public void SpawnVillagers(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            SpawnVillager();
        }
        
        OnUpdateVillagers.Invoke(VillagerCount);
        
        UCropoutGameInstance gameInstance = World.GameInstanceAs<UCropoutGameInstance>();
        gameInstance.UpdateAllVillagers();
    }

    public void SpawnVillager()
    {
        townHall.GetActorBounds(false, out FVector origin, out FVector extent);
        FVector offset = MathLibrary.RandomUnitVector() * double.Min(extent.X, extent.Y) * 2.0f;
        
        FVector spawnLocation = origin + offset;
        spawnLocation.Z = 0.0f;

        UNavigationSystemV1.GetRandomReachablePointInRadius(spawnLocation, out FVector vector, 500.0f);
        AIHelperLibrary.SpawnAIFromClass(VillagerClass.As<APawn>(), null, vector, FRotator.ZeroRotator);
        VillagerCount++;
    }
    
    public void StopMusic()
    {
        musicIsPlaying = false;
        UAudioModulationStatics.SetGlobalControlBusMixValue(CropoutPianoBus, 1.0f);
    }

    public void RemoveTargetResource(KeyValuePair<EResourceType, int> resource)
    {
        if (Resources.TryGetValue(resource.Key, out int currentAmount))
        {
            Resources[resource.Key] = currentAmount - resource.Value;
            OnResourceChanged.Invoke(resource.Key, Resources[resource.Key]);
        }
        
        if (Resources.TryGetValue(EResourceType.Food, out int foodAmount) && foodAmount == 0)
        {
            EndGame(false);
        }
    }
    
    public void RemoveResource(out KeyValuePair<EResourceType, int> resource)
    {
        throw new NotImplementedException();
    }

    public void AddResource(KeyValuePair<EResourceType, int> resource)
    {
        Resources[resource.Key] += resource.Value;
        OnResourceChanged.Invoke(resource.Key, Resources[resource.Key]);
        World.GameInstanceAs<UCropoutGameInstance>().UpdateAllResources(Resources);
    }

    public void RemoveCurrentActiveWidget()
    {
        if (GameWidget.IsValid)
        {
            GameWidget.PullCurrentStackItem();
        }
    }

    public IDictionary<EResourceType, int> GetCurrentResources()
    {
        return Resources;
    }
    
    public void AddUI(TSubclassOf<UCommonActivatableWidget> widget)
    {
        GameWidget.AddStackItem(widget);
    }

    public bool CheckResource(EResourceType resourceType, out int amount)
    {
        if (Resources.TryGetValue(resourceType, out amount))
        {
            return true;
        }
        
        amount = 0;
        return false;
    }
}