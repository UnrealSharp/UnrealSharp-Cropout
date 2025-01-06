using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CommonUI;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI.Elements;

[UClass]
public class UCropoutButton : UCommonButtonBase
{
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public UCommonActionWidget GamepadIcon { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public UCommonTextBlock ButtonTitle { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public USizeBox ButtonSize { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public FText ButtonText { get; set; }

    public override void PreConstruct(bool isDesignTime)
    {
        base.PreConstruct(isDesignTime);
        
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
}