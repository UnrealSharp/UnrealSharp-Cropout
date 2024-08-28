using UnrealSharp.Attributes;

namespace ManagedCropoutSampleProject.Core;

[UEnum]
public enum EInputType : byte
{
    Unknown,
    KeyMouse,
    Gamepad,
    Touch,
}