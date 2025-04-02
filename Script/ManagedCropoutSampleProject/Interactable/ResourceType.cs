using UnrealSharp.Attributes;

namespace ManagedCropoutSampleProject.Interactable;

[UEnum]
public enum EResourceType : byte
{
    None,
    Food,
    Wood,
    Stone,
    Max,
}