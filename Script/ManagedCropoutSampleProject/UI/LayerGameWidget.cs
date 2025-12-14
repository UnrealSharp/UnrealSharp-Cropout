using ManagedCropoutSampleProject.Core;
using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.UI.Elements;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CommonUI;
using UnrealSharp.Core.Attributes;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI;

[UClass]
public partial class ULayerGameWidget : UCommonActivatableWidget
{
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected partial UVerticalBox ResourceContainer { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected partial UCommonTextBlock VillagerCounter { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected partial UImage VillagerIcon { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected partial UCropoutButton BTN_Pause { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected partial UCommonActivatableWidgetStack MainStack { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public partial TSubclassOf<UPauseWidget> PauseWidgetClass { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public partial TSubclassOf<UEndGameWidget> EndGameWidgetClass { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public partial TSubclassOf<UResourceWidget> ResourceWidgetClass { get; set; }

    private EResourceType _currentResourceType = EResourceType.Food;
    private FTimerHandle _addResourceTimer;

    public override void Construct()
    {
        InitializeWidget();
        BTN_Pause.BindButtonClickedEvent(OnPaused);
        
        base.Construct();
    }

    public override void OnInitialized()
    {
        InitializeWidget();
        base.OnInitialized();
    }

    public void EndGame(bool win)
    {
        UEndGameWidget widget = MainStack.PushWidget(EndGameWidgetClass);
        widget.EndGame(win);
        widget.ActivateWidget();
    }

    void InitializeWidget()
    {
        ResourceContainer.ClearChildren();
        _addResourceTimer = SystemLibrary.SetTimer(AddResource, 1.0f, true);

        ACropoutGameMode cropoutGameMode = World.GameModeAs<ACropoutGameMode>();
        cropoutGameMode.OnUpdateVillagers += OnVillagersUpdated;

        ACropoutPlayerController cropoutPlayerController = OwningPlayerControllerAs<ACropoutPlayerController>();
        cropoutPlayerController.OnKeySwitch += OnKeySwitch;
    }
    
    public void AddStackItem(TSubclassOf<UCommonActivatableWidget> widgetClass)
    {
        UCommonActivatableWidget activatableWidget = MainStack.PushWidget(widgetClass);
        activatableWidget.ActivateWidget();
    }
    
    public void PullCurrentStackItem()
    {
        UCommonActivatableWidget widget = MainStack.ActiveWidget;
        
        if (widget is null)
        {
            return;
        }
        
        MainStack.RemoveWidget(widget);
    }
    
    [UFunction]
    void OnPaused(UCommonButtonBase buttonBase)
    {
        MainStack.PushWidget(PauseWidgetClass);
    }
    
    [UFunction]
    void OnVillagersUpdated(int villagerCount)
    {
        VillagerCounter.Text = villagerCount.ToString();
    }
    
    [UFunction]
    void OnKeySwitch(EInputType keySwitchType)
    {
        BTN_Pause.RenderOpacity = keySwitchType == EInputType.Gamepad ? 1.0f : 0.0f;
    }

    [UFunction]
    void AddResource()
    {
        if (!ResourceWidgetClass.IsValid)
        {
            throw new System.Exception("ResourceWidgetClass is not valid");
        }
        
        if (_currentResourceType == EResourceType.Max)
        {
            SystemLibrary.ClearAndInvalidateTimerHandle(ref _addResourceTimer);
        }
        else
        {
            UResourceWidget resourceWidget = CreateWidget(ResourceWidgetClass);
            resourceWidget.InitializeFrom(_currentResourceType);
            ResourceContainer.AddChildToVerticalBox(resourceWidget);
            
            _currentResourceType++;
        }
    }
}