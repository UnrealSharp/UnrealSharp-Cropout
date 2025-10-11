using ManagedCropoutSampleProject.AI;
using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.Core.Save;
using ManagedCropoutSampleProject.Interactable;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CommonUI;
using UnrealSharp.Core;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.EnhancedInput;
using UnrealSharp.InputCore;
using UnrealSharp.NavigationSystem;
using UnrealSharp.Niagara;

namespace ManagedCropoutSampleProject.Core;

[UClass]
public partial class ACropoutPlayer : APawn, IPlayer
{
    public ACropoutPlayer()
    {
        EdgeMoveDistance = 50.0f;
    }
    
    [UProperty(DefaultComponent = true, RootComponent = true)]
    public partial USceneComponent Root { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(Root))]
    public partial USpringArmComponent CameraBoom { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(CameraBoom))]
    public partial UCameraComponent PlayerCamera { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(Root))]
    public partial USphereComponent PlayerCollision { get; set; }
    
    [UProperty(DefaultComponent = true, AttachmentComponent = nameof(Root))]
    public partial UStaticMeshComponent CursorMesh { get; set; }
    
    [UProperty(DefaultComponent = true)]
    public partial UFloatingPawnMovement PawnMovement { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Camera")]
    public partial UCurveFloat CameraZoomCurve { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Camera")]
    public partial float EdgeMoveDistance { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Movement")]
    public partial UInputAction MoveAction { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Movement")]
    public partial UInputAction ZoomAction { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Movement")]
    public partial UInputAction SpinAction { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Movement")]
    public partial UInputAction DragMoveAction { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Mapping Context")]
    public partial UInputMappingContext DragMoveMappingContext { get; set; }

    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Mapping Context")]
    public partial UInputMappingContext BaseInputMappingContext { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Mapping Context")]
    public partial UInputMappingContext VillagerModeMappingContext { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly, Category = "Mapping Context")]
    public partial UInputMappingContext BuildModeMappingContext { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Interaction")]
    public partial UInputAction VillagerModeAction { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere, Category = "Interaction")]
    public partial UInputAction BuildMove { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly, Category = "Interaction")]
    public partial AActor? HoveredActor { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly | PropertyFlags.EditDefaultsOnly, Category = "Interaction")]
    public partial UNiagaraSystem? TargetEffect { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public partial UMaterialParameterCollection CropoutMaterialCollection { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public partial UMaterialInterface PlaceableMaterial { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public partial UStaticMesh PlaceableOverlayMesh { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly)]
    private partial UStaticMeshComponent? SpawnOverlay { get; set; }
    
    APlayerController PlayerController => (APlayerController) Controller;

    private float _zoomValue = 0.5f;
    private float _zoomDirection;
    private FVector _targetHandle;
    private FVector _storedMove;
    private EInputType _inputType = EInputType.KeyMouse;

    private TSubclassOf<AInteractable> _targetSpawnClass;
    private IDictionary<EResourceType, int>? _resourceCost;
    public AInteractable? Spawn;
    
    private bool _canDrop;

    private AActor? _villagerAction;
    private AActor? _selected;
    
    private UNiagaraComponent? _targetEffectComponent;
    private FTimerHandle _updatePathHandle;
    
    private FTimerHandle _closestHoverCheckHandle;
    private bool _isInBuildMode = false;

    protected override void BeginPlay_Implementation()
    {
        UpdateZoom();
        SetupInput();
        SystemLibrary.SetTimer(this, "UpdateMovement", 0.01f, true);
        base.BeginPlay_Implementation();
    }

    protected override void Possessed_Implementation(AController newController)
    {
        ACropoutPlayerController newPlayerController = (ACropoutPlayerController) newController;
        newPlayerController.OnKeySwitch += OnKeySwitch;
        base.Possessed_Implementation(newController);
    }

    private void SetupInput()
    {
        if (InputComponent is not UEnhancedInputComponent enhancedInputComponent)
        {
            return;
        }
        
        AddMappingContext(BaseInputMappingContext);
        AddMappingContext(VillagerModeMappingContext);
        
        enhancedInputComponent.BindAction(MoveAction, ETriggerEvent.Triggered, Move);
        enhancedInputComponent.BindAction(ZoomAction, ETriggerEvent.Triggered, Zoom);
        enhancedInputComponent.BindAction(SpinAction, ETriggerEvent.Triggered, Spin);
        enhancedInputComponent.BindAction(DragMoveAction, ETriggerEvent.Triggered, DragMove);
        
        enhancedInputComponent.BindAction(VillagerModeAction, ETriggerEvent.Triggered, VillagerMode_Triggered);
        enhancedInputComponent.BindAction(VillagerModeAction, ETriggerEvent.Started, VillagerMode_Started);
        enhancedInputComponent.BindAction(VillagerModeAction, ETriggerEvent.Canceled, VillagerMode_Canceled);
        enhancedInputComponent.BindAction(VillagerModeAction, ETriggerEvent.Completed, VillagerMode_Completed);
        
        enhancedInputComponent.BindAction(BuildMove, ETriggerEvent.Triggered, Build_Move_Triggered);
        enhancedInputComponent.BindAction(BuildMove, ETriggerEvent.Completed, Build_Move_Completed);
    }
    
    [UFunction]
    void Build_Move_Triggered(FInputActionValue value, float f, float arg3, UInputAction arg4)
    {
        UpdateBuildAsset();
    }
    
    [UFunction]
    void Build_Move_Completed(FInputActionValue value, float f, float arg3, UInputAction arg4)
    {
        if (Spawn == null)
        {
            return;
        }
        
        Spawn.SetActorLocation(ConvertToSteppedPos(Spawn.ActorLocation), false, out _, false);
    }

    [UFunction]
    void VillagerMode_Triggered(FInputActionValue value, float f, float arg3, UInputAction arg4)
    {
        _villagerAction = HoveredActor;
    }
    
    [UFunction]
    void VillagerMode_Started(FInputActionValue value, float f, float arg3, UInputAction arg4)
    {
        if (!SingleTouchCheck())
        {
            return;
        }
        
        PositionCheck();
        
        if (VillagerOverlapCheck(out APawn? overlappedActor))
        {
            VillagerSelect(overlappedActor!);
        }
        else
        {
            AddMappingContext(DragMoveMappingContext);
        }
    }
    
    [UFunction]
    void VillagerMode_Canceled(FInputActionValue value, float f, float arg3, UInputAction arg4)
    {
        VillagerMode_Completed(value, f, arg3, arg4);
    }
    
    [UFunction]
    void VillagerMode_Completed(FInputActionValue value, float f, float arg3, UInputAction arg4)
    {
        RemoveMappingContext(DragMoveMappingContext);

        if (_villagerAction != null && _villagerAction.IsValid && _selected is IVillager villager)
        {
            villager.Action(_villagerAction);
            
            SystemLibrary.ClearAndInvalidateTimerHandle(ref _updatePathHandle);
            
            if (_targetEffectComponent != null)
            {
                _targetEffectComponent.DestroyComponent(this);
            }
            
            _selected = null;
        }
        
        _villagerAction = null;
    }

    [UFunction]
    void Move(FInputActionValue value, float arg2, float arg3, UInputAction action)
    {
        FVector2D input = value.GetAxis2D();
        AddMovementInput(ActorForwardVector, input.Y.ToFloat());
        AddMovementInput(ActorRightVector, input.X.ToFloat());
    }
    
    [UFunction]
    void DragMove(FInputActionValue value, float arg2, float arg3, UInputAction action)
    {
        TrackMove();
    }

    [UFunction]
    void Zoom(FInputActionValue value, float arg2, float arg3, UInputAction action)
    {
        _zoomDirection = value.GetAxis1D();
        UpdateZoom();
    }
    
    [UFunction]
    void Spin(FInputActionValue value, float arg2, float arg3, UInputAction action)
    {
        AddActorLocalRotation(new FRotator(0.0f, value.GetAxis1D(), 0.0f), false, out _, false);
    }
    
    public void AddMappingContext(UInputMappingContext mappingContext, FModifyContextOptions options = default)
    {
        APlayerController playerController = UGameplayStatics.GetPlayerController(0);
        options.ForceImmediately = true;
        UEnhancedInputLocalPlayerSubsystem enhancedInputLocalPlayerSubsystem = GetLocalPlayerSubsystem<UEnhancedInputLocalPlayerSubsystem>(playerController);
        enhancedInputLocalPlayerSubsystem.AddMappingContext(mappingContext, 0, options);
    }
    
    public void RemoveMappingContext(UInputMappingContext mappingContext)
    {
        APlayerController playerController = UGameplayStatics.GetPlayerController(0);
        UEnhancedInputLocalPlayerSubsystem enhancedInputLocalPlayerSubsystem = GetLocalPlayerSubsystem<UEnhancedInputLocalPlayerSubsystem>(playerController);
        enhancedInputLocalPlayerSubsystem.RemoveMappingContext(mappingContext);
    }

    bool SingleTouchCheck()
    {
        if (PlayerController == null)
        {
            return false;
        }

        PlayerController.GetInputTouchState(ETouchIndex.Touch2, out _, out _, out bool bIsCurrentlyPressed);
        return !bIsCurrentlyPressed;
    }

    bool VillagerOverlapCheck(out APawn? overlappedActor)
    {
        GetOverlappingActors<APawn>(out IList<APawn> overlappingActors);
        
        if (overlappingActors.Count == 0)
        {
            overlappedActor = null;
            return false;
        }
        
        overlappedActor = overlappingActors[0];
        return true;
    }
    
    void VillagerSelect(AActor actor)
    {
        _selected = actor;

        if (TargetEffect == null)
        {
            PrintString("No Target Effect Set!");
            return;
        }
        
        _targetEffectComponent = UNiagaraFunctionLibrary.SpawnSystemAttached(TargetEffect, RootComponent, FName.None, FVector.Zero, FRotator.ZeroRotator, EAttachLocation.SnapToTarget, false);
        _updatePathHandle = SystemLibrary.SetTimer(this, "UpdatePath", 0.01f, true);
    }

    [UFunction]
    void UpdatePath()
    {
        if (_selected == null)
        {
            SystemLibrary.ClearAndInvalidateTimerHandle(ref _updatePathHandle);
            return;
        }
        
        UNavigationPath newPath = UNavigationSystemV1.FindPathToLocationSynchronously(PlayerCollision.WorldLocation, _selected.ActorLocation);
        
        if (newPath.PathPoints.Count == 0)
        {
            return;
        }

        FVector[] pathPoints = newPath.PathPoints.ToArray();
        
        pathPoints[0] = PlayerCollision.WorldLocation;
        pathPoints[pathPoints.Length - 1] = _selected.ActorLocation;
        
        UNiagaraDataInterfaceArrayFunctionLibrary.NiagaraSetVectorArray(_targetEffectComponent, "TargetPath", pathPoints);
    }

    protected override void ActorBeginOverlap_Implementation(AActor otherActor)
    {
        if (HoveredActor is null)
        {
            HoveredActor = otherActor;
        }

        _closestHoverCheckHandle = SystemLibrary.SetTimer(this, "ClosestHoverCheck", 0.01f, true);
        base.ActorBeginOverlap_Implementation(otherActor);
    }

    protected override void ActorEndOverlap_Implementation(AActor otherActor)
    {
         GetOverlappingActors<AActor>(out var overlappingActors);

        if (overlappingActors.Count == 0)
        {
            HoveredActor = null;
        }

        base.ActorEndOverlap_Implementation(otherActor);
    }

    [UFunction]
    void OnKeySwitch(EInputType newInput)
    {
        _inputType = newInput;
        
        switch (_inputType)
        {
            case EInputType.Gamepad:
                PlayerCollision.SetRelativeLocation(new FVector(0.0f, 0.0f, 10.0f), false, out _, false);
                CursorMesh.SetHiddenInGame(false, true);
                break;
            case EInputType.KeyMouse:
                CursorMesh.SetHiddenInGame(false, true);
                break;
            case EInputType.Touch:
                CursorMesh.SetHiddenInGame(true, true);
                PlayerCollision.SetRelativeLocation(new FVector(0.0f, 0.0f, -500.0f), false, out _, false);
                break;
        }
    }
    
    [UFunction]
    private void ClosestHoverCheck()
    {
        PlayerCollision.GetOverlappingActors(out IList<AActor> overlappingActors);
        
        if (overlappingActors.Count == 0)
        {
            SystemLibrary.ClearAndInvalidateTimerHandle(ref _closestHoverCheckHandle);
            return;
        }
        
        AActor? newHoveredActor = null;
        
        for (int i = 0; i < overlappingActors.Count; i++)
        {
            if (i == 0)
            {
                newHoveredActor = overlappingActors[i];
                continue;
            }
            
            FVector playerCollisionLocation = PlayerCollision.WorldLocation;
            if (FVector.Distance(overlappingActors[i].ActorLocation, playerCollisionLocation) 
                <FVector.Distance(playerCollisionLocation, overlappingActors[i].ActorLocation))
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
        ProjectMouseToGroundPlane(out _, out FVector intersection);
        _storedMove = _targetHandle - intersection;
        AddActorWorldOffset(new FVector(_storedMove.X, _storedMove.Y, 0), false, out _, false);
    }

    [UFunction]
    void UpdateMovement()
    {
        // Keep player within playspace
        FVector actorLocation = ActorLocation;
        double scaleValue = (actorLocation.Length() - 9000) / 5000;

        MathLibrary.Vector_Normalize(ref actorLocation);
        FVector worldDirection = new FVector(actorLocation.X, actorLocation.Y, 0) * -1.0;
        AddMovementInput(worldDirection, Math.Max(scaleValue, 0.0).ToFloat());
        
        // Syncs 3D Cursor and Collision Position
        UpdateCursorPosition();
        
        // Edge of screen movement
        EdgeMove(out FVector direction, out float strength);
        AddMovementInput(direction, strength);
        
        // Position Collision On Ground Plane Projection
        ProjectMouseToGroundPlane(out _, out FVector playerCollisionLocation);
        playerCollisionLocation.Z += 10.0f;
        PlayerCollision.SetWorldLocation(playerCollisionLocation, false, out _, false);
    }

    void EdgeMove(out FVector direction, out float strength)
    {
        PlayerController.GetViewportSize(out int width, out int height);
        FVector2D centerScreen = new FVector2D(width / 2.0, height / 2.0);
        
        ProjectMouseToGroundPlane(out FVector2D screenPosition, out FVector intersection);
        FVector2D cursorPosition = screenPosition - centerScreen;
        
        CursorDistanceFromViewportCenter(cursorPosition, out direction, out strength);
        direction = MathLibrary.TransformDirection(ActorTransform, direction);
    }

    private float GetEdgeMoveDistance()
    {
        float inputMultiplier;
        switch (_inputType)
        {
            case EInputType.KeyMouse:
                inputMultiplier = 1.0f;
                break;
            case EInputType.Gamepad:
                inputMultiplier = 2.0f;
                break;
            case EInputType.Touch:
                inputMultiplier = 2.0f;
                break;
            default:
                inputMultiplier = 0.0f;
                break;
        }
        return EdgeMoveDistance * inputMultiplier;
    }
    
    void CursorDistanceFromViewportCenter(FVector2D cursorPosition, out FVector direction, out float strength)
    {
        PlayerController.GetViewportSize(out int width, out int height);
        FVector2D viewportHalfSize = new FVector2D(width / 2.0, height / 2.0);
        
        // TODO: Negate operator for float
        viewportHalfSize.X -= GetEdgeMoveDistance();
        viewportHalfSize.Y -= GetEdgeMoveDistance();

        double xDeadZone = Math.Abs(cursorPosition.X) - viewportHalfSize.X;
        xDeadZone = Math.Max(xDeadZone, 0.0) / EdgeMoveDistance;
        
        double yDeadZone = Math.Abs(cursorPosition.Y) - viewportHalfSize.Y;
        yDeadZone = Math.Max(yDeadZone, 0.0) / EdgeMoveDistance;

        float signedX = Math.Sign(cursorPosition.X);
        float signedY = Math.Sign(cursorPosition.Y);
        
        direction = new FVector((signedY * yDeadZone) * -1.0f, signedX * xDeadZone, 0.0f);
        strength = 1.0f;
    }

    void UpdateCursorPosition()
    {
        if (_inputType is EInputType.KeyMouse or EInputType.Gamepad)
        {
            FTransform target = new FTransform();
            if (HoveredActor != null)
            {
                HoveredActor.GetActorBounds(true, out FVector origin, out FVector extent);
                target.Location = new FVector(origin.X, origin.Y, 20.0f);

                double x = MathLibrary.GetAbsMax2D(new FVector2D(extent.X, extent.Y)) / 50.0f;
                double y = Math.Sin(SystemLibrary.GameTimeInSeconds * 5.0f) * 0.25;

                double newScale = x + y + 1;
                target.Scale = new FVector(newScale, newScale, 1.0f);
            }
            else
            {
                target = PlayerCollision.WorldTransform;
                target.Scale.X = 2.0f;
                target.Scale.Y = 2.0f;
                target.Scale.Z = 1.0f;
            }
            
            double worldDeltaSeconds = UGameplayStatics.WorldDeltaSeconds;
            FTransform newTransform = MathLibrary.TInterpTo(CursorMesh.WorldTransform, target, worldDeltaSeconds.ToFloat(), 12.0f);
            CursorMesh.SetWorldTransform(newTransform, false, out _, false);
        }
    }

    void PositionCheck()
    {
        ProjectMouseToGroundPlane(out _, out FVector intersection);
        _targetHandle = intersection;
        if (_inputType == EInputType.Touch)
        {
            PlayerCollision.SetWorldLocation(_targetHandle, false, out _, false);
        }
    }
    
    [UFunction(FunctionFlags.BlueprintCallable)]
    public bool ProjectMouseToGroundPlane(out FVector2D screenPosition, out FVector intersection)
    {
        screenPosition = new FVector2D();
        bool isKeyMousePressed = false;
        bool isTouchPressed = false;
        
        switch (_inputType)
        {
            case EInputType.Unknown:
            case EInputType.Gamepad:
            {
                PlayerController.GetViewportSize(out int width, out int height);
                screenPosition = new FVector2D(width / 2.0, height / 2.0);
                break;
            }
            case EInputType.KeyMouse:
            {
                if (PlayerController.GetMousePosition(out float x, out float y))
                {
                    isKeyMousePressed = true;
                    screenPosition = new FVector2D(x, y);
                }
                else
                {
                    screenPosition = GetLockProjectionCenterScreen();
                }
                break;
            }
            case EInputType.Touch:
            {
                PlayerController.GetInputTouchState(ETouchIndex.Touch1, out float x, out float y, out isTouchPressed);
                if (isTouchPressed)
                {
                    screenPosition = new FVector2D(x, y);
                }
                else
                {
                    screenPosition = GetLockProjectionCenterScreen();
                }
                break;
            }
        }
        
        UGameplayStatics.DeprojectScreenToWorld(PlayerController, screenPosition, out FVector origin, out FVector direction);
        FPlane newPlane = MathLibrary.MakePlaneFromPointAndNormal(new FVector(), new FVector(0, 0, 1));
        MathLibrary.LinePlaneIntersection(origin, origin + direction * 100000, newPlane, out _, out intersection);

        if (_inputType == EInputType.Touch)
        {
            intersection.Z += isTouchPressed ? 0 : -500;
        }

        bool returnValue = false;
        switch (_inputType)
        {
            case EInputType.Unknown:
            {
                returnValue = false;
                break;
            }
            case EInputType.Gamepad:
            {
                returnValue = true;
                break;
            }
            case EInputType.KeyMouse:
            {
                returnValue = isKeyMousePressed;
                break;
            }
            case EInputType.Touch:
            {
                returnValue = isTouchPressed;
                break;
            }
        }

        return returnValue;
    }
    
    FVector2D GetLockProjectionCenterScreen()
    {
        PlayerController.GetViewportSize(out int width, out int height);
        return new FVector2D(width / 2.0, height / 2.0);
    }

    void UpdateZoom()
    {
        double alpha = CameraZoomCurve.GetFloatValue(_zoomValue).ToDouble();
        
        _zoomValue = Math.Clamp(_zoomDirection * 0.01f + _zoomValue, 0.0f, 1.0f);
        CameraBoom.TargetArmLength = MathLibrary.Lerp(800, 40000, alpha).ToFloat();
        
        double pitch = MathLibrary.Lerp(-40, -55, alpha);
        CameraBoom.SetRelativeRotation(new FRotator(pitch, 0, 0), false, out _, false);
        
        PawnMovement.MaxSpeed = MathLibrary.Lerp(1000, 6000, alpha).ToFloat();
        UpdateDepthOfField();
        PlayerCamera.FieldOfView = MathLibrary.Lerp(20, 15, alpha).ToFloat();
    }

    void UpdateDepthOfField()
    {
        FPostProcessSettings postProcessSettings = PlayerCamera.PostProcessSettings;
        postProcessSettings.FocalDistance = CameraBoom.TargetArmLength;
        postProcessSettings.DepthOfFieldSensorWidth = 150.0f;
        postProcessSettings.DepthOfFieldFstop = 3.0f;
        PlayerCamera.PostProcessSettings = postProcessSettings;
    }

    void CreateBuildOverlay()
    {
        if (SpawnOverlay != null)
        {
            return;
        }
        
        if (Spawn == null)
        {
            LogCropout.LogError("Spawn is null when trying to create build overlay");
            return;
        }
        
        GetActorBounds(false, out FVector origin, out FVector extent);
        FTransform buildTransform = new FTransform
        {
            Scale = extent / 50.0f,
        };
        
        SpawnOverlay = AddComponentByClass<UStaticMeshComponent>(true, buildTransform);
        
        SpawnOverlay.SetStaticMesh(PlaceableOverlayMesh);
        SpawnOverlay.SetMaterial(0, PlaceableMaterial);
        SpawnOverlay.RelativeScale3D = new FVector(5.0f);
        SpawnOverlay.SetCollisionProfileName("NoCollision");
        
        SpawnOverlay.AttachToComponent(Spawn.Mesh, FName.None, EAttachmentRule.SnapToTarget,
            EAttachmentRule.KeepWorld, EAttachmentRule.KeepWorld, true);
        
        
        UpdateBuildAsset();
    }

    void UpdateBuildAsset()
    {
        if (Spawn == null)
        {
            return;
        }

        if (!ProjectMouseToGroundPlane(out _, out FVector intersection))
        {
            return;
        }
        
        FVector targetLocation = ConvertToSteppedPos(intersection);
        FVector newLocation = MathLibrary.VInterpTo(Spawn.ActorLocation, targetLocation, UGameplayStatics.WorldDeltaSeconds.ToFloat(), 10.0f);
        Spawn.SetActorLocation(newLocation, false, out _, false);
        
        Spawn.GetOverlappingActors(out IList<AInteractable> overlappingInteractables);
        _canDrop = overlappingInteractables.Count == 0 && CornersInNav();

        FLinearColor color;
        color.R = targetLocation.X.ToFloat();
        color.G = targetLocation.Y.ToFloat();
        color.B = targetLocation.Z.ToFloat();
        color.A = _canDrop ? 1.0f : 0.0f;
        MaterialLibrary.SetVectorParameterValue(CropoutMaterialCollection, "Target Position", color);
    }

    bool CornersInNav()
    {
        if (Spawn == null)
        {
            LogCropout.LogError("Spawn is null when trying to check corners in nav");
            return false;
        }
        
        bool MultiLineTrace(FVector start, FVector end, out IList<FHitResult> hitResults)
        {
            return SystemLibrary.MultiLineTraceByChannel(start, end, ETraceChannel.Visibility.ToQuery(), false, 
                new List<AActor>(), EDrawDebugTrace.None, out hitResults, true);
        }
        
        SystemLibrary.GetComponentBounds(Spawn.Box, out FVector origin, out FVector extent, out float radius);
        
        double multipliedX = extent.X * 1.05f;
        double multipliedY = extent.Y * 1.05f;

        FVector start = origin;
        start.Z = 100;
        start.X += multipliedX;
        start.Y += multipliedY;
        
        FVector end = start;
        end.Z = -1;
        
        if (!MultiLineTrace(start, end, out IList<FHitResult> hitResults))
        {
            return false;
        }

        start = origin;
        start.X += multipliedX * -1;
        start.Y += multipliedY;
        start.Z = 100;
        
        end = start;
        end.Z = -1;
        
        if (!MultiLineTrace(start, end, out hitResults))
        {
            return false;
        }
        
        start = origin;
        start.X += multipliedY;
        start.Y += multipliedX * -1;
        start.Z = 100;
        
        end = start;
        end.Z = -1;
        
        if (!MultiLineTrace(start, end, out hitResults))
        {
            return false;
        }
        
        start = origin;
        start.X += multipliedY * -1;
        start.Y += multipliedX * -1;
        start.Z = 100;
        
        end = start;
        end.Z = -1;
        
        if (!MultiLineTrace(start, end, out hitResults))
        {
            return false;
        }
        
        return true;
    }

    FVector ConvertToSteppedPos(FVector A)
    {
        FVector b = A / 200.0f;
        b.X = Math.Round(b.X) * 200.0f;
        b.Y = Math.Round(b.Y) * 200.0f;
        return b;
    }

    public void BeginBuild(TSubclassOf<AInteractable> targetClass, IDictionary<EResourceType, int> resourceCost)
    {
        _targetSpawnClass = targetClass;
        _resourceCost = resourceCost;

        if (Spawn != null && Spawn.IsValid)
        {
            Spawn.DestroyActor();
        }
        
        FTransform spawnTransform = new FTransform
        {
            Location = ActorLocation
        };
        
        Spawn = SpawnActor(targetClass, spawnTransform);
        Spawn.PlacementMode();
        CreateBuildOverlay();
    }

    public void SpawnBuildTarget()
    {
        if (!_canDrop)
        {
            return;
        }
        
        if (Spawn == null)
        {
            LogCropout.LogError("Spawn is null when trying to spawn build target");
            return;
        }

        AInteractable interactable = SpawnActor(_targetSpawnClass, Spawn.ActorTransform);
        interactable.SetProgressionState(0);
        RemoveResources();
        
        IGameInstance gameInstance = World.GameInstanceAs<UCropoutGameInstance>();
        gameInstance.UpdateAllInteractables();
        UpdateBuildAsset();
    }

    public void RotateSpawn()
    {
        if (Spawn == null)
        {
            return;
        }
        
        Spawn.SetActorRotation(Spawn.ActorRotation + new FRotator(0.0f, 90.0f, 0.0f), false);
    }
    
    public void DestroySpawn()
    {
        if (Spawn == null || SpawnOverlay == null)
        {
            throw new NullReferenceException("Spawn or SpawnOverlay is null");
        }
        
        Spawn.DestroyActor();
        SpawnOverlay.DestroyComponent(this);
    }

    private void RemoveResources()
    {
        if (_resourceCost == null)
        {
            LogCropout.LogError("Resource cost is null when trying to remove resources");
            return;
        }
        
        ACropoutGameMode gameMode = World.GameModeAs<ACropoutGameMode>();
        foreach (KeyValuePair<EResourceType, int> resourceCost in _resourceCost)
        {
            gameMode.RemoveTargetResource(resourceCost);
        }

        IDictionary<EResourceType, int> currentResources = gameMode.GetCurrentResources();
        
        foreach (KeyValuePair<EResourceType, int> resource in _resourceCost)
        {
            int value = currentResources[resource.Key];
            
            if (value >= resource.Value)
            {
                continue;
            }
            
            gameMode.RemoveCurrentActiveWidget();
            DestroySpawn();
            break;
        }
    }

    public void SwitchBuildMode(bool switchBuildMode)
    {
        if (switchBuildMode == _isInBuildMode)
        {
            return;
        }
        
        if (switchBuildMode)
        {
            AddMappingContext(BuildModeMappingContext);
            RemoveMappingContext(VillagerModeMappingContext);
        }
        else
        {
            RemoveMappingContext(BuildModeMappingContext);
            AddMappingContext(VillagerModeMappingContext);
        }
        
        _isInBuildMode = switchBuildMode;
    }

    public void AddUI(TSubclassOf<UCommonActivatableWidget> widget)
    {
        throw new NotImplementedException();
    }

    public void RemoveCurrentUILayer()
    {
        throw new NotImplementedException();
    }
}