using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;
using UnrealSharp.InputCore;

namespace ManagedCropoutSampleProject.Core;

public partial class OnKeySwitch : MulticastDelegate<OnKeySwitch.OnKeySwitchDelegate>
{
    public delegate void OnKeySwitchDelegate(InputType newInput);
}

[UClass]
public class CropoutPlayerController : PlayerController
{
    protected override void ReceiveBeginPlay()
    {
        SetupInput();
        base.ReceiveBeginPlay();
    }
    [UProperty(PropertyFlags.BlueprintAssignable)]
    public OnKeySwitch OnKeySwitch { get; set; }
    
    [UFunction(FunctionFlags.BlueprintPure | FunctionFlags.BlueprintCallable)]
    public InputType GetInputType()
    {
        return _inputType;
    }
    
    private InputType _inputType = InputType.KeyMouse;

    public InputType InputType
    {
        set
        {
            _inputType = value;
            OnKeySwitch.Invoke(_inputType);
        }
    }

    void SetupInput()
    {
        InputComponent.BindAction("KeyDetect", EInputEvent.IE_Pressed, KeyDetect);
        InputComponent.BindAxis("MouseMove", MouseMove);
        InputComponent.BindAction("TouchDetect", EInputEvent.IE_Pressed, TouchDetect);
    }
    
    [UFunction]
    void KeyDetect(Key key)
    {
        if (InputLibrary.Key_IsGamepadKey(key))
        {
            switch (_inputType)
            {
                case InputType.Touch:
                case InputType.Unknown:
                case InputType.KeyMouse: 
                    InputType = InputType.Gamepad; 
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
                case InputType.Gamepad:
                case InputType.Unknown:
                case InputType.Touch:
                    InputType = InputType.KeyMouse;
                    break;
            }
        }
    }
    
    [UFunction]
    void TouchDetect()
    {
        switch (_inputType)
        {
            case InputType.Gamepad:
            case InputType.Unknown:
            case InputType.KeyMouse:
                InputType = InputType.Touch;
                break;
        }
    }
}