using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.EnhancedInput;
using UnrealSharp.InputCore;
using UnrealSharp.SlateCore;

namespace ManagedCropoutSampleProject.Core;

[UClass]
public class Player_CS : Pawn
{
    [UProperty(DefaultComponent = true, RootComponent = true)]
    public SceneComponent Root { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(Root))]
    public SpringArmComponent CameraBoom { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(CameraBoom))]
    public CameraComponent PlayerCamera { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(Root))]
    public SphereComponent PlayerCollision { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(Root))]
    public StaticMeshComponent CursorMesh { get; set; }
    
    [UProperty(DefaultComponent = true)]
    public FloatingPawnMovement PawnMovement { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public CurveFloat CameraZoomCurve { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public InputAction MoveAction { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public InputAction ZoomAction { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public InputAction SpinAction { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public Actor? HoveredActor { get; set; }

    private float _zoomValue = 0.5f;
    private float _zoomDirection;
    private Vector _targetHandle;
    private Vector _storedMove;

    private InputType _inputType = InputType.KeyMouse;

    protected override void ReceiveBeginPlay()
    {
        UpdateZoom();
        SetupInput();
        SystemLibrary.SetTimer(UpdateMovement, 0.01f, true);
        base.ReceiveBeginPlay();
    }
    
    PlayerController PlayerController => (PlayerController) Controller;
    
    [UFunction]
    private void UpdateCameraMovement()
    {
        ProjectMouseToGroundPlane(out _, out Vector intersection);

        Vector XOffset = CameraBoom.GetForwardVector() * (CameraBoom.TargetArmLength - CameraBoom.SocketOffset.X) * -1.0f;
        Vector ZOffset = CameraBoom.GetUpVector() * CameraBoom.SocketOffset.Z;
        Vector newLocation = (XOffset + ZOffset + CameraBoom.GetWorldLocation()) - PlayerCamera.GetWorldLocation();
        
        _storedMove = _targetHandle - intersection - newLocation;
        AddActorWorldOffset(new Vector(_storedMove.X, _storedMove.Y, 0), false, out _, false);
    }

    [UFunction]
    void UpdateMovement()
    {
        Vector actorLocation = GetActorLocation();
        double scaleValue = (actorLocation.Length() - 9000) / 5000;

        MathLibrary.Normalize(ref actorLocation);
        Vector worldDirection = new Vector(actorLocation.X, actorLocation.Y, 0) * -1.0;
        AddMovementInput(worldDirection, Math.Max(scaleValue, 0.0).ToFloat());
        UpdateCursorPosition();
        
        ProjectMouseToGroundPlane(out _, out Vector playerCollisionLocation);
        playerCollisionLocation.Z += 10.0f;
        
        PlayerCollision.SetWorldLocation(playerCollisionLocation, false, out _, false);
        
        SystemLibrary.DrawDebugSphere(this, CursorMesh.GetWorldLocation(), 50, 12, LinearColor.Green);
    }

    void UpdateCursorPosition()
    {
        if (_inputType is InputType.KeyMouse or InputType.Gamepad)
        {
            Transform target = new Transform();
            if (HoveredActor != null)
            {
                HoveredActor.GetActorBounds(true, out Vector origin, out Vector extent);
                target.Translation = new Vector(origin.X, origin.Y, 20.0f);

                double x = MathLibrary.GetAbsMax(new Vector2D(extent.X, extent.Y)) / 50.0f;
                double y = Math.Sin(SystemLibrary.GetGameTimeInSeconds(this) * 5.0f) * 0.25;

                double newScale = x + y + 1;
                target.Scale3D = new Vector(newScale, newScale, 1.0f);
            }
            else
            {
                target = PlayerCollision.GetWorldTransform();
                target.Scale3D.X = 2.0f;
                target.Scale3D.Y = 2.0f;
                target.Scale3D.Z = 1.0f;
            }
            
            double worldDeltaSeconds = GameplayStatics.GetWorldDeltaSeconds(this);
            Transform newTransform = MathLibrary.InterpTo(CursorMesh.GetWorldTransform(), target, worldDeltaSeconds.ToFloat(), 12.0f);
            CursorMesh.SetWorldTransform(newTransform, false, out _, false);
        }
    }
    
    bool ProjectMouseToGroundPlane(out Vector2D screenPosition, out Vector intersection)
    {
        screenPosition = new Vector2D();
        bool isKeyMousePressed = false;
        bool isTouchPressed = false;
        
        switch (_inputType)
        {
            case InputType.Unknown:
            case InputType.Gamepad:
            {
                PlayerController.GetViewportSize(out int width, out int height);
                screenPosition = new Vector2D(width / 2.0, height / 2.0);
                break;
            }
            case InputType.KeyMouse:
            {
                if (PlayerController.GetMousePosition(out float x, out float y))
                {
                    isKeyMousePressed = true;
                    screenPosition = new Vector2D(x, y);
                }
                else
                {
                    screenPosition = GetLockProjectionCenterScreen();
                }
                break;
            }
            case InputType.Touch:
            {
                PlayerController.GetInputTouchState(ETouchIndex.Touch1, out float x, out float y, out isTouchPressed);
                if (isTouchPressed)
                {
                    screenPosition = new Vector2D(x, y);
                }
                else
                {
                    screenPosition = GetLockProjectionCenterScreen();
                }
                break;
            }
        }
        
        GameplayStatics.DeprojectScreenToWorld(PlayerController, screenPosition, out Vector origin, out Vector direction);
        Plane newPlane = MathLibrary.MakePlaneFromPointAndNormal(new Vector(), new Vector(0, 0, 1));
        MathLibrary.LinePlaneIntersection(origin, origin + direction * 100000, newPlane, out _, out intersection);

        if (_inputType == InputType.Touch)
        {
            intersection.Z += isTouchPressed ? 0 : -500;
        }

        bool returnValue = false;
        switch (_inputType)
        {
            case InputType.Unknown:
            {
                returnValue = false;
                break;
            }
            case InputType.Gamepad:
            {
                returnValue = true;
                break;
            }
            case InputType.KeyMouse:
            {
                returnValue = isKeyMousePressed;
                break;
            }
            case InputType.Touch:
            {
                returnValue = isTouchPressed;
                break;
            }
        }

        return returnValue;
    }
    
    Vector2D GetLockProjectionCenterScreen()
    {
        PlayerController.GetViewportSize(out int width, out int height);
        return new Vector2D(width / 2.0, height / 2.0);
    }

    private void SetupInput()
    {
        if (InputComponent is not EnhancedInputComponent enhancedInputComponent)
        {
            return;
        }
        
        enhancedInputComponent.BindAction(MoveAction, ETriggerEvent.Triggered, Move);
        enhancedInputComponent.BindAction(ZoomAction, ETriggerEvent.Triggered, Zoom);
        enhancedInputComponent.BindAction(SpinAction, ETriggerEvent.Triggered, Spin);
    }

    [UFunction]
    void Move(InputActionValue value)
    {
        Vector2D input = value.GetAxis2D();
        AddMovementInput(GetActorForwardVector(), input.Y.ToFloat());
        AddMovementInput(GetActorRightVector(), input.X.ToFloat());
    }

    [UFunction]
    void Zoom(InputActionValue value)
    {
        _zoomDirection = value.GetAxis1D();
        UpdateZoom();
    }
    
    [UFunction]
    void Spin(InputActionValue value)
    {
        AddActorLocalRotation(new Rotator(0.0f, value.GetAxis1D(), 0.0f), false, out _, false);
    }

    void UpdateZoom()
    {
        double alpha = CameraZoomCurve.GetFloatValue(_zoomValue).ToDouble();
        
        _zoomValue = Math.Clamp(_zoomDirection * 0.01f + _zoomValue, 0.0f, 1.0f);
        CameraBoom.TargetArmLength = MathLibrary.Lerp(800, 40000, alpha).ToFloat();
        
        double pitch = MathLibrary.Lerp(-40, -55, alpha);
        CameraBoom.SetRelativeRotation(new Rotator(pitch, 0, 0), false, out _, false);
        
        PawnMovement.MaxSpeed = MathLibrary.Lerp(1000, 6000, alpha).ToFloat();
        UpdateDepthOfField();
        PlayerCamera.SetFieldOfView(MathLibrary.Lerp(20, 15, alpha).ToFloat());
    }

    void UpdateDepthOfField()
    {
        PostProcessSettings postProcessSettings = PlayerCamera.PostProcessSettings;
        postProcessSettings.DepthOfFieldFocalDistance = CameraBoom.TargetArmLength;
        postProcessSettings.DepthOfFieldSensorWidth = 150.0f;
        postProcessSettings.DepthOfFieldFstop = 3.0f;
        PlayerCamera.PostProcessSettings = postProcessSettings;
    }
}