using ManagedCropoutSampleProject.Core.Save;
using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.UI;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.AudioModulation;
using UnrealSharp.CommonUI;
using UnrealSharp.Engine;

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
    
    private bool musicIsPlaying;
    private bool hasEndGame;
    
    protected override void BeginPlay()
    {
        base.BeginPlay();
        CreateGameHUD();
        
        UCropoutGameInstance gameInstance = World.GameInstanceAs<UCropoutGameInstance>();
        gameInstance.UpdateAllResources(Resources);
        
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
    
    public void StopMusic()
    {
        musicIsPlaying = false;
        UAudioModulationStatics.SetGlobalControlBusMixValue(CropoutPianoBus, 1.0f);
    }

    public void RemoveResource(KeyValuePair<EResourceType, int> resource)
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