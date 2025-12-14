using ManagedCropoutSampleProject.Core;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CommonUI;
using UnrealSharp.Core.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.SlateCore;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI.Elements;

[UClass]
public partial class UBuildConfirmWidget : UCommonActivatableWidget
{
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public partial UCropoutButton BTN_Place { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public partial UCropoutButton BTN_Rotate { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public partial UCropoutButton BTN_Cancel { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public partial UCommonBorder CommonBorder_1 { get; set; }

    private FVectorSpringState SpringState;
    ACropoutPlayer Player => OwningPlayerPawnAs<ACropoutPlayer>();

    public override void Construct()
    {
        BTN_Place.BindButtonClickedEvent(OnClickPlace);
        BTN_Cancel.BindButtonClickedEvent(OnClickCancel);
        BTN_Rotate.BindButtonClickedEvent(OnClickRotate);
        base.Construct();
    }

    public override void OnActivated()
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

    public override void Tick(FGeometry myGeometry, float deltaTime)
    {
        base.Tick(myGeometry, deltaTime);

        FVector2D current2D = CommonBorder_1.Transform.Translation;
        FVector current = new FVector(current2D.X, current2D.Y, 0.0f);
        
        FVector2D adjustPosition = AdjustPosition();
        FVector target = new FVector(adjustPosition.X, adjustPosition.Y, 0.0f);
        
        FVector newPosition = MathLibrary.VectorSpringInterp(current, target, ref SpringState, 400.0f, 0.5f, UGameplayStatics.WorldDeltaSeconds.ToFloat(), 1.0f, 0.75f);
        
        FWidgetTransform widgetTransform = CommonBorder_1.Transform;
        widgetTransform.Translation = new FVector2D(newPosition.X, newPosition.Y);
        CommonBorder_1.Transform = widgetTransform;
    }

    FVector2D AdjustPosition()
    {
        ACropoutPlayer pawn = (ACropoutPlayer) OwningPlayerController.ControlledPawn;
        
        if (pawn.Spawn == null)
        {
            return FVector2D.Zero;
        }
        
        UGameplayStatics.ProjectWorldToScreen(OwningPlayerController, pawn.Spawn.ActorLocation, out FVector2D screenPosition, true);

        double viewportScale = UWidgetLayoutLibrary.ViewportScale.ToDouble();
        FVector2D viewportSize = UWidgetLayoutLibrary.ViewportSize;
        
        FVector2D scaledScreenPos = screenPosition / viewportScale;
        
        double scaledX = viewportSize.X / viewportScale;
        double scaledY = viewportSize.Y / viewportScale;
        
        double maxX = double.Clamp(scaledScreenPos.X, 150.0f, scaledX - 150.0f);
        double maxY = double.Clamp(scaledScreenPos.Y, 0, scaledY - 350.0f);
        
        return new FVector2D(maxX, maxY);
    }
    
    [UFunction]
    private void OnClickRotate(UCommonButtonBase button)
    {
        Player.RotateSpawn();
    }
    
    [UFunction]
    private void OnClickCancel(UCommonButtonBase button)
    {
        Player.DestroySpawn();
        DeactivateWidget();
    }
    
    [UFunction]
    private void OnClickPlace(UCommonButtonBase button)
    {
        Player.SpawnBuildTarget();
    }
}