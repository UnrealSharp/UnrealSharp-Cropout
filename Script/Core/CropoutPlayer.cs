using ManagedCropoutSampleProject.Villagers;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.EnhancedInput;
using UnrealSharp.InputCore;
using UnrealSharp.NavigationSystem;
using UnrealSharp.Niagara;
using UnrealSharp.SlateCore;

namespace ManagedCropoutSampleProject.Core;

[UClass]
public class CropoutPlayer : Pawn
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
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Camera")]
    public CurveFloat CameraZoomCurve { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Camera")]
    public float EdgeMoveDistance { get; set; } = 50.0f;
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Movement")]
    public InputAction MoveAction { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Movement")]
    public InputAction ZoomAction { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Movement")]
    public InputAction SpinAction { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Movement")]
    public InputAction DragMoveAction { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Mapping Context")]
    public InputMappingContext DragMoveMappingContext { get; set; }

    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Mapping Context")]
    public InputMappingContext BaseInputMappingContext { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Mapping Context")]
    public InputMappingContext VillagerModeMappingContext { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Interaction")]
    public InputAction VillagerModeAction { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly, Category = "Interaction")]
    public Actor? HoveredActor { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly, Category = "Interaction")]
    public NiagaraSystem? TargetEffect { get; set; }
    
    PlayerController? PlayerController => (PlayerController) Controller;

    private float _zoomValue = 0.5f;
    private float _zoomDirection;
    private Vector _targetHandle;
    private Vector _storedMove;
    private InputType _inputType = InputType.KeyMouse;

    private Actor? _villagerAction;
    private Actor? _selected;
    
    private NiagaraComponent _targetEffectComponent;
    private TimerHandle updatePathHandle;
    
    private TimerHandle _closestHoverCheckHandle;

    protected override void ReceiveBeginPlay()
    {
        UpdateZoom();
        SetupInput();
        SystemLibrary.SetTimer(UpdateMovement, 0.01f, true);
        base.ReceiveBeginPlay();
    }

    public override void ReceivePossessed(Controller newController)
    {
        CropoutPlayerController newPlayerController = (CropoutPlayerController) newController;
        newPlayerController.OnKeySwitch += OnKeySwitch;
        
        base.ReceivePossessed(newController);
    }
    
    private void SetupInput()
    {
        if (InputComponent is not EnhancedInputComponent enhancedInputComponent)
        {
            return;
        }
        
        AddMappingContext(BaseInputMappingContext);
        AddMappingContext(VillagerModeMappingContext);
        
        enhancedInputComponent.BindAction(MoveAction, ETriggerEvent.Triggered, Move);
        enhancedInputComponent.BindAction(ZoomAction, ETriggerEvent.Triggered, Zoom);
        enhancedInputComponent.BindAction(SpinAction, ETriggerEvent.Triggered, Spin);
        enhancedInputComponent.BindAction(DragMoveAction, ETriggerEvent.Triggered, DragMove);
        return;
        
        enhancedInputComponent.BindAction(VillagerModeAction, ETriggerEvent.Triggered, VillagerMode_Triggered);
        enhancedInputComponent.BindAction(VillagerModeAction, ETriggerEvent.Started, VillagerMode_Started);
        enhancedInputComponent.BindAction(VillagerModeAction, ETriggerEvent.Canceled, VillagerMode_Canceled);
        enhancedInputComponent.BindAction(VillagerModeAction, ETriggerEvent.Completed, VillagerMode_Completed);
    }
    
    [UFunction]
    void VillagerMode_Triggered(InputActionValue value)
    {
        _villagerAction = HoveredActor;
    }
    
    [UFunction]
    void VillagerMode_Started(InputActionValue value)
    {
        if (!SingleTouchCheck())
        {
            return;
        }
        
        PositionCheck();
        
        if (VillagerOverlapCheck(out Actor? overlappedActor))
        {
            VillagerSelect(overlappedActor!);
        }
        else
        {
            AddMappingContext(DragMoveMappingContext);
        }
    }
    
    [UFunction]
    void VillagerMode_Canceled(InputActionValue value)
    {
        VillagerMode_Completed(value);
    }
    
    [UFunction]
    void VillagerMode_Completed(InputActionValue value)
    {
        ModifyContextOptions modifyContextOptions = new ModifyContextOptions
        {
            IgnoreAllPressedKeysUntilRelease = true,
            ForceImmediately = true,
            NotifyUserSettings = false
        };

        RemoveMappingContext(DragMoveMappingContext, modifyContextOptions);

        if (_villagerAction != null && _villagerAction.IsValid && _selected is IVillager villager)
        {
            villager.Action(_villagerAction);
            
            SystemLibrary.ClearAndInvalidateTimerHandle(this, ref updatePathHandle);
            _targetEffectComponent.DestroyComponent(this);
            _selected = null;
        }
        
        _villagerAction = null;
    }

    [UFunction]
    void Move(InputActionValue value)
    {
        Vector2D input = value.GetAxis2D();
        AddMovementInput(GetActorForwardVector(), input.Y.ToFloat());
        AddMovementInput(GetActorRightVector(), input.X.ToFloat());
    }
    
    [UFunction]
    void DragMove(InputActionValue value)
    {
        TrackMove();
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
    
    // TODO: Support BlueprintCallable interface methods from C++.
    [UFunction(FunctionFlags.BlueprintEvent)]
    public extern void AddMappingContext(InputMappingContext mappingContext, ModifyContextOptions options = default);
    
    // TODO: Support BlueprintCallable interface methods from C++.
    [UFunction(FunctionFlags.BlueprintEvent)]
    public extern void RemoveMappingContext(InputMappingContext mappingContext, ModifyContextOptions options = default);

    bool SingleTouchCheck()
    {
        if (PlayerController == null)
        {
            return false;
        }

        PlayerController.GetInputTouchState(ETouchIndex.Touch1, out _, out _, out bool bIsCurrentlyPressed);
        return !bIsCurrentlyPressed;
    }

    bool VillagerOverlapCheck(out Actor? overlappedActor)
    {
        GetOverlappingActors(out IList<Actor> overlappingActors);
        
        if (overlappingActors.Count == 0)
        {
            overlappedActor = null;
            return false;
        }
        
        overlappedActor = overlappingActors[0];
        return true;
    }
    
    void VillagerSelect(Actor actor)
    {
        _selected = actor;

        if (TargetEffect == null)
        {
            PrintString("No Target Effect Set!");
            return;
        }
        
        _targetEffectComponent = NiagaraFunctionLibrary.SpawnSystemAttached(TargetEffect, RootComponent, Name.None, Vector.Zero, Rotator.ZeroRotator, EAttachLocation.SnapToTarget, false);
        updatePathHandle = SystemLibrary.SetTimer(UpdatePath, 0.01f, true);
    }

    [UFunction]
    void UpdatePath()
    {
        if (_selected == null)
        {
            SystemLibrary.ClearAndInvalidateTimerHandle(this, ref updatePathHandle);
            return;
        }


        NavigationPath newPath = NavigationSystemV1.FindPathToLocationSynchronously(this, PlayerCollision.GetWorldLocation(), _selected.GetActorLocation());
        
        if (newPath.PathPoints.Count == 0)
        {
            return;
        }

        Vector[] pathPoints = newPath.PathPoints.ToArray();
        
        pathPoints[0] = PlayerCollision.GetWorldLocation();
        pathPoints[pathPoints.Length - 1] = _selected.GetActorLocation();
        
        NiagaraDataInterfaceArrayFunctionLibrary.SetNiagaraArrayVector(_targetEffectComponent, "TargetPath", pathPoints);
    }

    public override void ReceiveActorBeginOverlap(Actor otherActor)
    {
        if (HoveredActor is null)
        {
            HoveredActor = otherActor;
        }

        _closestHoverCheckHandle = SystemLibrary.SetTimer(ClosestHoverCheck, 0.01f, true);
        base.ReceiveActorBeginOverlap(otherActor);
    }

    public override void ReceiveActorEndOverlap(Actor otherActor)
    {
        GetOverlappingActors(out var overlappingActors);
        
        if (overlappingActors.Count == 0)
        {
            HoveredActor = null;
        }
        
        base.ReceiveActorEndOverlap(otherActor);
    }
    
    [UFunction]
    void OnKeySwitch(InputType newInput)
    {
        _inputType = newInput;
        
        switch (_inputType)
        {
            case InputType.Gamepad:
                PlayerCollision.SetRelativeLocation(new Vector(0.0f, 0.0f, 10.0f), false, out _, false);
                CursorMesh.SetHiddenInGame(false, true);
                break;
            case InputType.KeyMouse:
                CursorMesh.SetHiddenInGame(false, true);
                break;
            case InputType.Touch:
                CursorMesh.SetHiddenInGame(true, true);
                PlayerCollision.SetRelativeLocation(new Vector(0.0f, 0.0f, -500.0f), false, out _, false);
                break;
        }
    }
    
    [UFunction]
    private void ClosestHoverCheck()
    {
        PlayerCollision.GetOverlappingActors(out IList<Actor> overlappingActors);
        
        if (overlappingActors.Count == 0)
        {
            SystemLibrary.ClearAndInvalidateTimerHandle(this, ref _closestHoverCheckHandle);
            return;
        }
        
        Actor? newHoveredActor = null;
        
        for (int i = 0; i < overlappingActors.Count; i++)
        {
            if (i == 0)
            {
                newHoveredActor = overlappingActors[i];
                continue;
            }
            
            Vector playerCollisionLocation = PlayerCollision.GetWorldLocation();
            if (Vector.Distance(overlappingActors[i].GetActorLocation(), playerCollisionLocation) 
                < Vector.Distance(playerCollisionLocation, overlappingActors[i].GetActorLocation()))
            {
                newHoveredActor = overlappingActors[i];
            }
        }
        
        if (!Equals(newHoveredActor, HoveredActor))
        {
            HoveredActor = newHoveredActor;
        }
    }

    [UFunction]
    private void TrackMove()
    {
        ProjectMouseToGroundPlane(out _, out Vector intersection);

        Vector xOffset = CameraBoom.GetForwardVector() * (CameraBoom.TargetArmLength - CameraBoom.SocketOffset.X) * -1.0f;
        Vector zOffset = CameraBoom.GetUpVector() * CameraBoom.SocketOffset.Z;
        Vector newLocation = (xOffset + zOffset + CameraBoom.GetWorldLocation()) - PlayerCamera.GetWorldLocation();
        
        _storedMove = _targetHandle - intersection - newLocation;
        AddActorWorldOffset(new Vector(_storedMove.X, _storedMove.Y, 0), false, out _, false);
    }

    [UFunction]
    void UpdateMovement()
    {
        // Keep player within playspace
        Vector actorLocation = GetActorLocation();
        double scaleValue = (actorLocation.Length() - 9000) / 5000;

        MathLibrary.Normalize(ref actorLocation);
        Vector worldDirection = new Vector(actorLocation.X, actorLocation.Y, 0) * -1.0;
        AddMovementInput(worldDirection, Math.Max(scaleValue, 0.0).ToFloat());
        
        // Syncs 3D Cursor and Collision Position
        UpdateCursorPosition();
        
        // Edge of screen movement
        EdgeMove(out Vector direction, out float strength);
        AddMovementInput(direction, strength);
        
        // Position Collision On Ground Plane Projection
        ProjectMouseToGroundPlane(out _, out Vector playerCollisionLocation);
        playerCollisionLocation.Z += 10.0f;
        PlayerCollision.SetWorldLocation(playerCollisionLocation, false, out _, false);
    }

    void EdgeMove(out Vector direction, out float strength)
    {
        PlayerController.GetViewportSize(out int width, out int height);
        Vector2D centerScreen = new Vector2D(width / 2.0, height / 2.0);
        
        ProjectMouseToGroundPlane(out Vector2D screenPosition, out Vector intersection);
        Vector2D cursorPosition = screenPosition - centerScreen;
        
        CursorDistanceFromViewportCenter(cursorPosition, out direction, out strength);
        direction = MathLibrary.TransformDirection(GetActorTransform(), direction);
    }

    private float GetEdgeMoveDistance()
    {
        float inputMultiplier;
        switch (_inputType)
        {
            case InputType.KeyMouse:
                inputMultiplier = 1.0f;
                break;
            case InputType.Gamepad:
                inputMultiplier = 2.0f;
                break;
            case InputType.Touch:
                inputMultiplier = 2.0f;
                break;
            default:
                inputMultiplier = 0.0f;
                break;
        }
        return EdgeMoveDistance * inputMultiplier;
    }
    
    void CursorDistanceFromViewportCenter(Vector2D cursorPosition, out Vector direction, out float strength)
    {
        PlayerController.GetViewportSize(out int width, out int height);
        Vector2D viewportHalfSize = new Vector2D(width / 2.0, height / 2.0);
        
        // TODO: Negate operator for float
        viewportHalfSize.X -= GetEdgeMoveDistance();
        viewportHalfSize.Y -= GetEdgeMoveDistance();

        double xDeadZone = Math.Abs(cursorPosition.X) - viewportHalfSize.X;
        xDeadZone = Math.Max(xDeadZone, 0.0) / EdgeMoveDistance;
        
        double yDeadZone = Math.Abs(cursorPosition.Y) - viewportHalfSize.Y;
        yDeadZone = Math.Max(yDeadZone, 0.0) / EdgeMoveDistance;

        float signedX = Math.Sign(cursorPosition.X);
        float signedY = Math.Sign(cursorPosition.Y);
        
        direction = new Vector((signedY * yDeadZone) * -1.0f, signedX * xDeadZone, 0.0f);
        strength = 1.0f;
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

    void PositionCheck()
    {
        _targetHandle = Vector.Zero;
        if (_inputType == InputType.Touch)
        {
            PlayerCollision.SetWorldLocation(_targetHandle, false, out _, false);
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