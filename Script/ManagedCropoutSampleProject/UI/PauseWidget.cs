using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.Core.Save;
using ManagedCropoutSampleProject.UI.Elements;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI;

[UClass]
public class UPauseWidget : UCommonActivatableWidget
{
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public UCropoutButton BTN_Resume { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public UCropoutButton BTN_Restart { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public UCropoutButton BTN_MainMenu { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public USliderWidget Slider_Music { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public USliderWidget Slider_SFX { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public TSoftObjectPtr<UWorld> MainMenuLevel { get; set; }

    public override void Construct()
    {
        base.Construct();
        
        BTN_Resume.BindButtonClickedEvent(OnResume);
        BTN_Restart.BindButtonClickedEvent(OnRestart);
        BTN_MainMenu.BindButtonClickedEvent(OnMainMenu);
        
        Slider_Music.UpdateSlider();
        Slider_SFX.UpdateSlider();
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        
        WidgetLibrary.SetInputModeUIOnly(OwningPlayerController, BP_GetDesiredFocusTarget(), EMouseLockMode.DoNotLock);
        UGameplayStatics.SetGamePaused(true);
    }

    [UFunction]
    private void OnResume(UCommonButtonBase buttonBase)
    {
        UGameplayStatics.SetGamePaused(false);
        WidgetLibrary.SetInputMode_GameOnly(OwningPlayerController);
        DeactivateWidget();
    }
    
    [UFunction]
    private void OnRestart(UCommonButtonBase buttonBase)
    {
        UGameplayStatics.SetGamePaused(false);

        IGameInstance gameInstance = (IGameInstance) GameInstance;
        gameInstance.ClearSave(false);
        gameInstance.LoadLevel();
    }
    
    [UFunction]
    private void OnMainMenu(UCommonButtonBase buttonBase)
    {
        UGameplayStatics.SetGamePaused(false);

        UCropoutGameInstance gameInstance = World.GameInstanceAs<UCropoutGameInstance>();
        gameInstance.ClearSave(true);
        gameInstance.OpenLevel(MainMenuLevel);
    }
}