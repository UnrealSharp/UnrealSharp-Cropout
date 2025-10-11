using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.UI.Elements;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;
using UnrealSharp.Core;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI;

[UClass]
public partial class UMainMenuWidget : UCommonActivatableWidget
{
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public partial UCropoutButton BTN_Continue { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public partial UCropoutButton BTN_NewGame { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public partial UCropoutButton BTN_Quit { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public partial UCropoutButton BTN_Donate { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected partial TSoftObjectPtr<UWorld> MainLevel { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected partial TSubclassOf<UPromptWidget> PromptWidgetClass { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public partial FText OverwriteGamePromptText { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public partial FText QuitGamePromptText { get; set; }
    
    private bool _hasSave;
    private UCommonActivatableWidgetStack? _stack;
    
    protected override void OnActivated_Implementation()
    {
        base.OnActivated_Implementation();
        
        BP_GetDesiredFocusTarget().SetFocus();

        UCropoutGameInstance gameInstance = (UCropoutGameInstance)GameInstance;
        _hasSave = gameInstance.HasSave;
        BTN_Continue.IsEnabled = _hasSave;

        string platformName = UGameplayStatics.PlatformName;
        bool isIOSOrAndroid = platformName == "IOS" || platformName == "Android";
        BTN_Donate.Visibility = isIOSOrAndroid ? ESlateVisibility.Visible : ESlateVisibility.Collapsed;
    }

    protected override UWidget BP_GetDesiredFocusTarget_Implementation()
    {
        return _hasSave ? BTN_Continue : BTN_NewGame;
    }

    protected override void Construct_Implementation()
    {
        base.Construct_Implementation();
        BTN_Continue.BindButtonClickedEvent(OnClickContinue);
        BTN_NewGame.BindButtonClickedEvent(OnClickNewGame);
        BTN_Quit.BindButtonClickedEvent(OnClickQuit);
    }
    
    public void InitializeFrom(UCommonActivatableWidgetStack mainStack)
    {
        _stack = mainStack;
    }

    [UFunction]
    private void OnClickContinue(UCommonButtonBase button)
    {
        UCropoutGameInstance gameInstance = (UCropoutGameInstance)GameInstance;
        gameInstance.OpenLevel(MainLevel);
    }
    
    [UFunction]
    private void OnClickNewGame(UCommonButtonBase button)
    {
        if (_hasSave && _stack != null)
        {
            UPromptWidget promptWidget = _stack.PushWidget(PromptWidgetClass);
            promptWidget.InitializeFrom(OverwriteGamePromptText, OnClickConfirm, null);
        }
        else
        {
            OnClickConfirm();
        }
    }

    [UFunction]
    private void OnClickConfirm()
    {
        UCropoutGameInstance gameInstance = (UCropoutGameInstance)GameInstance;
        gameInstance.ClearSave(true);
        gameInstance.OpenLevel(MainLevel);
    }
    
    [UFunction]
    private void OnClickQuit(UCommonButtonBase button)
    {
        if (_stack == null)
        {
            LogCropout.LogWarning("Main menu stack is null, quitting immediately");
            return;
        }
        
        UPromptWidget promptWidget = _stack.PushWidget(PromptWidgetClass);
        promptWidget.InitializeFrom(OverwriteGamePromptText, OnConfirmQuit, null);
    }

    [UFunction]
    private void OnConfirmQuit()
    {
        SystemLibrary.QuitGame(null, EQuitPreference.Quit, false);
    }
}