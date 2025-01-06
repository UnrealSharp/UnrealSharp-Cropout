// ManagedCropoutSampleProject, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// ManagedCropoutSampleProject.Interactable.FResourceInfo

using ManagedCropoutSampleProject.Interactable;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

[UStruct]
public struct FResourceInfo
{
	[UProperty(PropertyFlags.EditDefaultsOnly)]
	public FText Title;

	[UProperty(PropertyFlags.EditDefaultsOnly)]
	public TSoftObjectPtr<AInteractable> TargetClass;

	[UProperty(PropertyFlags.EditDefaultsOnly)]
	public TSoftObjectPtr<UTexture2D> UIIcon;

	[UProperty(PropertyFlags.EditDefaultsOnly)]
	public TMap<EResourceType, int> Cost;

	[UProperty(PropertyFlags.EditDefaultsOnly)]
	public FColor TabColor;
}
