﻿using ManagedCropoutSampleProject.Core;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CommonUI;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI.Elements;

[UClass]
public class UBuildConfirmWidget : UCommonActivatableWidget
{
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public UCropoutButton BTN_Place { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public UCropoutButton BTN_Rotate { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public UCropoutButton BTN_Cancel { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public UCommonBorder CommonBorder_1 { get; set; }

    protected override void OnActivated()
    {
        base.OnActivated();

        ACropoutPlayerController playerController = OwningPlayerAs<ACropoutPlayerController>();
        playerController.OnNewInput(playerController.InputType);
        playerController.OnKeySwitch += playerController.OnNewInput;
        
        APawn pawn = playerController.ControlledPawn;
        pawn.EnableInput(playerController);
        pawn.ActorTickEnabled = true;
        WidgetLibrary.SetFocusToGameViewport();

        FWidgetTransform transform;
        transform.Translation = AdjustPosition();
        transform.Scale = FVector2D.One;
        transform.Shear = FVector2D.Zero;
        transform.Angle = 0.0f;
        CommonBorder_1.Transform = transform;
    }

    [UFunction]
    private void OnClickPos()
    {
        ACropoutPlayer player = OwningPlayerPawnAs<ACropoutPlayer>();
        player.SpawnBuildTarget();
    }

    FVector2D AdjustPosition()
    {
        APlayerController playerController = UGameplayStatics.GetPlayerController(0);
        ACropoutPlayer pawn = (ACropoutPlayer) playerController.ControlledPawn;
        UGameplayStatics.ProjectWorldToScreen(playerController, pawn.spawn!.ActorLocation, out FVector2D screenPosition);

        double viewportScale = UWidgetLayoutLibrary.ViewportScale.ToDouble();
        FVector2D viewportSize = UWidgetLayoutLibrary.ViewportSize;
        
        FVector2D scaledScreenPos = screenPosition / viewportScale;
        
        double scaledX = viewportSize.X * viewportScale;
        double scaledY = viewportSize.Y * viewportScale;
        
        double maxX = double.Clamp(scaledScreenPos.X, 150.0f, scaledX - 150.0f);
        double maxY = double.Clamp(scaledScreenPos.Y, 150.0f, scaledY - 350.0f);
        
        return new FVector2D(maxX, maxY);
    }
}