using ManagedCropoutSampleProject.Core;
using ManagedCropoutSampleProject.UI.Elements;
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
    public UCropoutButton BuildButton { get; set; }

    public override void Construct()
    {
        base.Construct();

        ACropoutPlayerController playerController = OwningPlayerControllerAs<ACropoutPlayerController>();
        SetInputMode(playerController.InputType);
        
        playerController.OnKeySwitch += SetInputMode;
        
        playerController.ControlledPawn.EnableInput(playerController);
        playerController.ControlledPawn.ActorTickEnabled = true;
        WidgetLibrary.SetFocusToGameViewport();
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
}