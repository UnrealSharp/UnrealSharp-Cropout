using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Core;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Interactable;

[UStruct]
public partial struct FResourceInfo
{
	[UProperty(PropertyFlags.EditDefaultsOnly)]
	public FText Title;

	[UProperty(PropertyFlags.EditDefaultsOnly)]
	public TSoftClassPtr<AInteractable> TargetClass;

	[UProperty(PropertyFlags.EditDefaultsOnly)]
	public TSoftObjectPtr<UTexture2D> UIIcon;

	[UProperty(PropertyFlags.EditDefaultsOnly)]
	public IDictionary<EResourceType, int> Cost;

	[UProperty(PropertyFlags.EditDefaultsOnly)]
	public FColor TabColor;
}
