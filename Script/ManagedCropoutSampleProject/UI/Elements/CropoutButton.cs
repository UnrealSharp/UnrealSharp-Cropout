using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CommonUI;
using UnrealSharp.Core;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI.Elements;

[UClass]
public partial class UCropoutButton : UCommonButtonBase
{
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public partial UCommonActionWidget GamepadIcon { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public partial UCommonTextBlock ButtonTitle { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public partial USizeBox ButtonSize { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public partial FText ButtonText { get; set; }

    protected override void PreConstruct_Implementation(bool isDesignTime)
    {
        base.PreConstruct_Implementation(isDesignTime);
        
        ButtonTitle.Text = ButtonText;

        float minHeight;
        string platformName = UGameplayStatics.PlatformName;
        if (platformName is "Android" or "IOS")
        {
            minHeight = MinHeight * 1.5f;
        }
        else
        {
            minHeight = MinHeight;
        }
        
        ButtonSize.MinDesiredHeight = minHeight;
        GamepadIcon.InputAction = TriggeringInputAction;
    }
    
    public void BindButtonClickedEvent(CommonButtonBaseClicked onButtonBaseClicked)
    {
        OnButtonBaseClicked += onButtonBaseClicked;
    }
}