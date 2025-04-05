using ManagedCropoutSampleProject.Core;
using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.UI.Common;
using ManagedCropoutSampleProject.UI.Elements;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI;

[UClass]
public class UGameMainWidget : UCommonActivatableWidget
{
    [UProperty, BindWidget]
    protected UCropoutButton BuildButton { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected TSubclassOf<UCommonActivatableWidget> BuildWidgetClass { get; set; }

    public override void Construct()
    {
        base.Construct();

        ACropoutPlayerController playerController = OwningPlayerControllerAs<ACropoutPlayerController>();
        SetInputMode(playerController.InputType);
        
        playerController.OnKeySwitch += SetInputMode;
        
        playerController.ControlledPawn.EnableInput(playerController);
        playerController.ControlledPawn.ActorTickEnabled = true;
        WidgetLibrary.SetFocusToGameViewport();
        
        BuildButton.BindButtonClickedEvent(OnBuildButtonClicked);
    }

    [UFunction]
    void SetInputMode(EInputType inputType)
    {
        APlayerController playerController = OwningPlayerController;
        playerController.ShowMouseCursor = false;
        switch (inputType)
        {
            case EInputType.Unknown:
                WidgetLibrary.SetInputModeGameAndUI(playerController);
                WidgetLibrary.SetFocusToGameViewport();
                break;
            case EInputType.KeyMouse:
                playerController.ShowMouseCursor = true;
                WidgetLibrary.SetInputModeGameAndUI(playerController);
                break;
            case EInputType.Gamepad:
                WidgetLibrary.SetInputMode_GameOnly(playerController);
                WidgetLibrary.SetFocusToGameViewport();
                break;
            case EInputType.Touch:
                WidgetLibrary.SetInputModeGameAndUI(playerController);
                WidgetLibrary.SetFocusToGameViewport();
                break;
        }
    }
    
    [UFunction]
    public void OnBuildButtonClicked(UCommonButtonBase buttonBase)
    {
        ACropoutGameMode gameMode = World.GameModeAs<ACropoutGameMode>();
        gameMode.AddUI(BuildWidgetClass);
    }
}