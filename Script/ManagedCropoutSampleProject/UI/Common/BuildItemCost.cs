using ManagedCropoutSampleProject.Interactable;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI.Common;

[UClass]
public partial class UBuildItemCostWidget : UUserWidget
{
    [UProperty, BindWidget]
    protected partial UCommonTextBlock C_Cost { get; set; }
    
    [UProperty, BindWidget]
    protected partial UCommonLazyImage C_Icon { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected partial TMap<EResourceType, TSoftObjectPtr<UTexture2D>> ResourceIcons { get; set; }
    
    public void InitializeFromResourceCost(KeyValuePair<EResourceType, int> resourceCost)
    {
        if (ResourceIcons.TryGetValue(resourceCost.Key, out TSoftObjectPtr<UTexture2D> value))
        {
            C_Icon.SetBrushFromLazyTexture(value);
        }
        
        C_Cost.Text = resourceCost.Value.ToString();
    }
}