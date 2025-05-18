using ManagedCropoutSampleProject.Core.Save;
using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.UI;
using UnrealSharp;
using UnrealSharp.Attributes;
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
    
    public bool HasSave { get; private set; }
    
    private bool MusicPlaying;
    private UTransitionWidget TransitionWidgetInstance;
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
            MusicPlaying = false;
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

    [UFunction(FunctionFlags.BlueprintCallable)]
    public async void OpenLevel(TSoftObjectPtr<UWorld> level)
    {
        try
        {
            Transition(ETransitionType.In);
            await Task.Delay(1100).ConfigureWithUnrealContext();
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