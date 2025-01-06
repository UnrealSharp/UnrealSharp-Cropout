using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;
using UnrealSharp.InputCore;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.Core;

/// <summary>
/// Multicast delegate  (see https://www.unrealsharp.com/delegates.html)
/// </summary>
[UMultiDelegate]
public delegate void OnKeySwitchDelegate(EInputType newInput);

[UClass]
public class ACropoutPlayerController : APlayerController
{
    [UProperty(PropertyFlags.BlueprintAssignable)]
    public TMulticastDelegate<OnKeySwitchDelegate> OnKeySwitch { get; set; }


    [UFunction(FunctionFlags.BlueprintPure | FunctionFlags.BlueprintCallable)]
    public EInputType GetInputType()
    {
        return _inputType;
    }
    
    private EInputType _inputType = Core.EInputType.KeyMouse;

    public EInputType InputType
    {
        get => _inputType;
        set
        {
            _inputType = value;
            OnKeySwitch.InnerDelegate.Invoke(_inputType);
        }
    }
    
    protected override void BeginPlay()
    {
        SetupInput();
        base.BeginPlay();
    }

    public void SetupInput()
    {
        InputComponent.BindAction("KeyDetect", EInputEvent.IE_Pressed, KeyDetect);
        InputComponent.BindAxis("MouseMove", MouseMove);
        InputComponent.BindAction("TouchDetect", EInputEvent.IE_Pressed, TouchDetect);
    }

    [UFunction]
    public void OnNewInput(EInputType newInput)
    {
        APlayerController controller = UGameplayStatics.GetPlayerController(0);
        controller.ShowMouseCursor = false;
        
        switch (newInput)
        {
            case EInputType.Gamepad:
                WidgetLibrary.SetInputMode_GameOnly(controller);
                break;
            case EInputType.KeyMouse:
                controller.ShowMouseCursor = true;
                break;
            default:
            case EInputType.Touch:
                WidgetLibrary.SetInputModeGameAndUI(controller, null);
                break;
        }
        
        WidgetLibrary.SetFocusToGameViewport();
    }
    
    [UFunction]
    void KeyDetect(FKey key)
    {
        if (InputLibrary.IsGamepadKey(key))
        {
            switch (_inputType)
            {
                case EInputType.Touch:
                case EInputType.Unknown:
                case EInputType.KeyMouse: 
                    InputType = EInputType.Gamepad; 
                    break;
            }
        }
    }
    
    [UFunction]
    void MouseMove(float axisValue)
    {
        if (axisValue != 0)
        {
            switch (_inputType)
            {
                case EInputType.Gamepad:
                case EInputType.Unknown:
                case EInputType.Touch:
                    InputType = EInputType.KeyMouse;
                    break;
            }
        }
    }
    
    [UFunction]
    void TouchDetect()
    {
        switch (_inputType)
        {
            case EInputType.Gamepad:
            case EInputType.Unknown:
            case EInputType.KeyMouse:
                InputType = EInputType.Touch;
                break;
        }
    }
}