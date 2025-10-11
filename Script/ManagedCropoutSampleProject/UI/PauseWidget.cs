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
public partial class UPauseWidget : UCommonActivatableWidget
{
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public partial UCropoutButton BTN_Resume { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public partial UCropoutButton BTN_Restart { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public partial UCropoutButton BTN_MainMenu { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public partial USliderWidget Slider_Music { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public partial USliderWidget Slider_SFX { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public partial TSoftObjectPtr<UWorld> MainMenuLevel { get; set; }

    protected override void Construct_Implementation()
    {
        base.Construct_Implementation();
        
        BTN_Resume.BindButtonClickedEvent(OnResume);
        BTN_Restart.BindButtonClickedEvent(OnRestart);
        BTN_MainMenu.BindButtonClickedEvent(OnMainMenu);
        
        Slider_Music.UpdateSlider();
        Slider_SFX.UpdateSlider();
    }

    protected override void OnActivated_Implementation()
    {
        base.OnActivated_Implementation();
        
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