using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.UI.Elements;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.AudioModulation;
using UnrealSharp.CommonUI;
using UnrealSharp.Core;
using UnrealSharp.Core.Attributes;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI;

[UClass, UMetaData("Test", "Test"), UMetaData("Tester")]
public partial class UEndGameWidget : UCommonActivatableWidget
{
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected partial UCommonTextBlock MainText { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected partial UCropoutButton BTN_Retry { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected partial UCropoutButton BTN_Continue { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected partial UCropoutButton BTN_MainMenu { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected partial FText WinText { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected partial FText LoseText { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected partial TSoftObjectPtr<UWorld> MainMenuLevel { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected partial TSoftObjectPtr<UWorld> VillageLevel { get; set; }

    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected partial USoundControlBus CropoutMusicBus { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public partial bool CallSavjje { get; set; }
    
    private bool _isWin = false;

    public override void Construct()
    {
        base.Construct();
        BTN_Continue.BindButtonClickedEvent(OnContinue);
        BTN_Retry.BindButtonClickedEvent(OnRetry);
        BTN_MainMenu.BindButtonClickedEvent(OnMainMenu);
    }

    public override void OnActivated()
    {
        WidgetLibrary.SetInputModeUIOnly(OwningPlayerController, BP_GetDesiredFocusTarget(), EMouseLockMode.DoNotLock);
        OwningPlayerPawn.DisableInput(OwningPlayerController);
        UGameplayStatics.SetGamePaused(true);
        base.OnActivated();
    }

    public override UWidget BP_GetDesiredFocusTarget()
    {
        return BTN_Continue;
    }

    public void EndGame(bool win)
    {
        _isWin = win;
        
        MainText.Text = win ? WinText : LoseText;
        UAudioModulationStatics.SetGlobalControlBusMixValue(CropoutMusicBus, win ? 1.0f : 0.0f);

        ACropoutGameMode gamemode = World.GameModeAs<ACropoutGameMode>();
        gamemode.StopMusic();
    }
    
    [UFunction]
    public void OnRetry(UCommonButtonBase buttonBase)
    {
        UCropoutGameInstance gameInstance = (UCropoutGameInstance) GameInstance;
        gameInstance.ClearSave(false);
        gameInstance.OpenLevel(VillageLevel);
    }
    
    [UFunction]
    public void OnContinue(UCommonButtonBase buttonBase)
    {
        UGameplayStatics.SetGamePaused(false);
        DeactivateWidget();
    }
    
    [UFunction]
    public void OnMainMenu(UCommonButtonBase buttonBase)
    {
        UCropoutGameInstance gameInstance = (UCropoutGameInstance) GameInstance;
        gameInstance.OpenLevel(MainMenuLevel);
        UGameplayStatics.SetGamePaused(false);
     }
}