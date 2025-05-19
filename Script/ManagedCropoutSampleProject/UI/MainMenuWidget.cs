using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.UI.Elements;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI;

[UClass]
public class UMainMenuWidget : UCommonActivatableWidget
{
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public UCropoutButton BTN_Continue { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public UCropoutButton BTN_NewGame { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public UCropoutButton BTN_Quit { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public UCropoutButton BTN_Donate { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected TSoftObjectPtr<UWorld> MainLevel { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected TSubclassOf<UPromptWidget> PromptWidgetClass { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public FText OverwriteGamePromptText { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public FText QuitGamePromptText { get; set; }
    
    private bool hasSave;
    private UCommonActivatableWidgetStack Stack;
    
    protected override void OnActivated()
    {
        base.OnActivated();
        
        BP_GetDesiredFocusTarget().SetFocus();

        UCropoutGameInstance gameInstance = (UCropoutGameInstance)GameInstance;
        hasSave = gameInstance.HasSave;
        BTN_Continue.IsEnabled = hasSave;

        string platformName = UGameplayStatics.PlatformName;
        bool isIOSOrAndroid = platformName == "IOS" || platformName == "Android";
        BTN_Donate.Visibility = isIOSOrAndroid ? ESlateVisibility.Visible : ESlateVisibility.Collapsed;
    }

    protected override UWidget BP_GetDesiredFocusTarget()
    {
        return hasSave ? BTN_Continue : BTN_NewGame;
    }

    public override void Construct()
    {
        base.Construct();
        
        BTN_Continue.BindButtonClickedEvent(OnClickContinue);
        BTN_NewGame.BindButtonClickedEvent(OnClickNewGame);
        BTN_Quit.BindButtonClickedEvent(OnClickQuit);
    }
    
    public void InitializeFrom(UCommonActivatableWidgetStack mainStack)
    {
        Stack = mainStack;
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
        if (hasSave)
        {
            UPromptWidget promptWidget = Stack.PushWidget(PromptWidgetClass);
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
        UPromptWidget promptWidget = Stack.PushWidget(PromptWidgetClass);
        promptWidget.InitializeFrom(OverwriteGamePromptText, OnConfirmQuit, null);
    }

    [UFunction]
    private void OnConfirmQuit()
    {
        SystemLibrary.QuitGame(null, EQuitPreference.Quit, false);
    }
}