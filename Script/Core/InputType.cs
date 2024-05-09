using UnrealSharp.Attributes;

namespace ManagedCropoutSampleProject.Core;

[UEnum]
public enum InputType : byte
{
    Unknown,
    KeyMouse,
    Gamepad,
    Touch,
}