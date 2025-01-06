using ManagedCropoutSampleProject.Interactable;
using UnrealSharp.Attributes;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI.Common;

[UClass]
public class UBuildItemCostWidget : UUserWidget
{
    private KeyValuePair<EResourceType, int> ResourceCost;
    
    public void InitializeFromResourceCost(KeyValuePair<EResourceType, int> resourceCost)
    {
        ResourceCost = resourceCost;
    }
}