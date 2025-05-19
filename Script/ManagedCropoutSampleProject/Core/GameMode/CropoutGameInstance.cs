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
        LoadGame();
        TransitionWidgetInstance = CreateWidget(TransitionWidget);
    }

    void LoadGame()
    {
        if (UGameplayStatics.DoesSaveGameExist("SAVE", 0))
        {
            SaveObject = (UCropoutSaveObject)UGameplayStatics.LoadGameFromSlot("SAVE", 0);
        }
        else
        {
            SaveObject = UGameplayStatics.CreateSaveGameObject<UCropoutSaveObject>();
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
    }

    public void UpdateAllInteractables()
    {
        SaveObject.Interactables.Clear();

        UGameplayStatics.GetAllActorsOfClass<AInteractable>(out IList<AInteractable> interactables);

        for (int i = 0; i < interactables.Count; i++)
        {
            AInteractable interactable = interactables[i];
            
            FInteractableSaveData entry;
            entry.Transform = interactable.ActorTransform;
            entry.Type = interactable.Class;
            entry.Health = interactable.ProgressionState;
            entry.Tag = interactable.Tags.Count > 0 ? interactable.Tags[0] : FName.None;
            
            SaveObject.Interactables.Add(entry);
        }
    }

    public void UpdateAllResources(TMap<EResourceType, int> resources)
    {
        SaveObject = UGameplayStatics.CreateSaveGameObject<UCropoutSaveObject>();
        SaveObject.Resources.Add(EResourceType.None, 700);
        Console.WriteLine(SaveObject.Resources[EResourceType.None]);
    }

    public void SaveGame()
    {
        UAsyncActionHandleSaveGame saveGameAction =
            UAsyncActionHandleSaveGame.AsyncSaveGameToSlot(SaveObject, "SAVE", 0);

        saveGameAction.Completed += [UFunction](USaveGame saveGame, bool success) => { HasSave = true; };

        saveGameAction.Activate();
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