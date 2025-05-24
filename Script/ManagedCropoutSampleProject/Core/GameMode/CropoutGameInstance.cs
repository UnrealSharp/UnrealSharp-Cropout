using ManagedCropoutSampleProject.Core.Save;
using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.UI;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.AudioModulation;
using UnrealSharp.CommonUI;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Core.GameMode;

[UEnum]
public enum ETransitionType : byte
{
    In,
    Out
}

[UClass]
public class UCropoutGameInstance : UGameInstance, IGameInstance, IPlayer
{
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    TSubclassOf<UTransitionWidget> TransitionWidget { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public UCropoutSaveObject SaveObject { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected USoundControlBus WinLoseBus { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected USoundControlBus MusicStopBus { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected USoundControlBus PianoBus { get; set; }
    
    [UProperty]
    private UAudioComponent Audio { get; set; }
    
    [UProperty]
    private UTransitionWidget TransitionWidgetInstance { get; set; }
    
    public bool HasSave { get; private set; }
    private bool _musicPlaying;
    public float[] SoundMixes { get; } = [1.0f, 1.0f];

    public override void Init()
    {
        TransitionWidgetInstance = CreateWidget(TransitionWidget);
        LoadGame();
    }

    void LoadGame()
    {
        if (UGameplayStatics.DoesSaveGameExist("SAVE", 0))
        {
            SaveObject = (UCropoutSaveObject) UGameplayStatics.LoadGameFromSlot("SAVE", 0);
            HasSave = true;
        }
        else
        {
            SaveObject = UGameplayStatics.CreateSaveGameObject<UCropoutSaveObject>();
            SaveObject.RandomStream = new FRandomStream(MathLibrary.RandomInteger(2147483647));
            _musicPlaying = false;
        }
    }

    [UFunction(FunctionFlags.BlueprintCallable)]
    public void Transition(ETransitionType transitionType)
    {
        if (!TransitionWidgetInstance.IsInViewport())
        {
            TransitionWidgetInstance.AddToViewport();
        }

        if (transitionType == ETransitionType.In)
        {
            TransitionWidgetInstance.TransitionIn();
        }
        else
        {
            TransitionWidgetInstance.TransitionOut();
        }
    }
    
    public async void OpenLevel(TSoftObjectPtr<UWorld> level)
    {
        try
        {
            Transition(ETransitionType.In);
            await Task.Delay(TimeSpan.FromSeconds(1.1)).ConfigureWithUnrealContext();
            UGameplayStatics.OpenLevelBySoftObjectPtr(level);
        }
        catch (Exception e)
        {
            LogCropout.Log(e.Message);
        }
    }

    public void UpdateAllVillagers()
    {
        SaveObject.Villagers.Clear();
        
        UGameplayStatics.GetAllActorsOfClass<ACropoutVillager>(out IList<ACropoutVillager> pawns);
        foreach (ACropoutVillager pawn in pawns)
        {
            FVillagerSaveData entry = new FVillagerSaveData
            {
                Transform = pawn.ActorLocation,
                Task = pawn.Tags[0],
            };
            
            SaveObject.Villagers.Add(entry);
        }
        
        SaveGame();
    }

    public void UpdateAllInteractables()
    {
        SaveObject.Interactables.Clear();

        UGameplayStatics.GetAllActorsOfClass<AInteractable>(out IList<AInteractable> interactables);

        foreach (AInteractable interactable in interactables)
        {
            FInteractableSaveData entry;
            entry.Transform = interactable.ActorTransform;
            entry.Type = interactable.Class;
            entry.Health = interactable.ProgressionState;
            entry.Tag = interactable.Tags.Count > 0 ? interactable.Tags[0] : FName.None;
            
            SaveObject.Interactables.Add(entry);
        }
        
        SaveGame();
    }

    public void UpdateAllResources(TMap<EResourceType, int> resources)
    {
        SaveObject.Resources.Clear();
        
        foreach (KeyValuePair<EResourceType, int> resource in resources)
        {
            SaveObject.Resources.Add(resource.Key, resource.Value);
        }
        
        SaveGame();
    }

    public void SaveGame()
    {
        UAsyncActionHandleSaveGame saveGameAction =
            UAsyncActionHandleSaveGame.AsyncSaveGameToSlot(SaveObject, "SAVE", 0);
        saveGameAction.Completed += OnSaved;
        saveGameAction.Activate();
    }

    [UFunction]
    public void OnSaved(USaveGame saveGame, bool success)
    {
        HasSave = true;
    }

    [UFunction(FunctionFlags.BlueprintCallable)]
    public void PlayMusic(USoundBase sound, float volume, bool persist)
    {
        if (_musicPlaying)
        {
            return;
        }
        
        UAudioModulationStatics.SetGlobalControlBusMixValue(WinLoseBus, 0.5f, 0.0f);
        UAudioModulationStatics.SetGlobalControlBusMixValue(MusicStopBus, 0.0f, 0.0f);

        Audio = UGameplayStatics.CreateSound2D(sound, volume, 1.0f, 0.0f, null, persist);
        Audio.Play(0.0f);
        _musicPlaying = true;
    }

    public void StopMusic()
    {
        _musicPlaying = false;
        UAudioModulationStatics.SetGlobalControlBusMixValue(PianoBus, 1.0f, 0.0f);
    }

    public void ClearSave(bool clearSeed)
    {
        SaveObject.ClearSave();

        if (clearSeed)
        {
            int randomInt = MathLibrary.RandomInteger(2147483647);
            SaveObject.RandomStream = new FRandomStream(randomInt);
        }

        HasSave = false;
    }

    public void LoadLevel()
    {
        HasSave = true;
    }

    public void BeginBuild(TSubclassOf<AInteractable> targetClass, IDictionary<EResourceType, int> resourceCost)
    {
    }

    public void SwitchBuildMode(bool switchBuildMode)
    {
        throw new NotImplementedException();
    }

    public void AddUI(TSubclassOf<UCommonActivatableWidget> widget)
    {
        throw new NotImplementedException();
    }

    public void RemoveCurrentUILayer()
    {
        throw new NotImplementedException();
    }
}