using ManagedCropoutSampleProject.Core;
using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.UI.Elements;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI;

[UClass]
public class ULayerGameWidget : UCommonActivatableWidget
{
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected UVerticalBox ResourceContainer { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected UCommonTextBlock VillagerCounter { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected UImage VillagerIcon { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected UCropoutButton BTN_Pause { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected UCommonActivatableWidgetStack MainStack { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public TSubclassOf<UPauseWidget> PauseWidgetClass { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public TSubclassOf<UResourceWidget> ResourceWidgetClass { get; set; }
    
    private EResourceType _currentResourceType;
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
        MainStack.PushWidget(widgetClass);
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
        if (!ResourceWidgetClass.Valid)
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