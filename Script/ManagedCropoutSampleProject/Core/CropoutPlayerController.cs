using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;
using UnrealSharp.InputCore;

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

    void SetupInput()
    {
        InputComponent.BindAction("KeyDetect", EInputEvent.IE_Pressed, KeyDetect);
        InputComponent.BindAxis("MouseMove", MouseMove);
        InputComponent.BindAction("TouchDetect", EInputEvent.IE_Pressed, TouchDetect);
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