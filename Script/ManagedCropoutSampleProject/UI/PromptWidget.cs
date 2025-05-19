using ManagedCropoutSampleProject.UI.Elements;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI;

[UMultiDelegate]
public delegate void FOnPromptResponse();

[UClass]
public class UPromptWidget : UCommonActivatableWidget
{
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected UCommonTextBlock Title { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected UCropoutButton BTN_Pos { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected UCropoutButton BTN_Neg { get; set; }
    
    [UProperty(PropertyFlags.BlueprintAssignable)]
    protected TMulticastDelegate<FOnPromptResponse> OnConfirm { get; set; }
    
    [UProperty(PropertyFlags.BlueprintAssignable)]
    protected TMulticastDelegate<FOnPromptResponse> OnBack { get; set; }

    public override void Construct()
    {
        base.Construct();
        
        BTN_Pos.BindButtonClickedEvent(OnClickPos);
        BTN_Neg.BindButtonClickedEvent(OnClickNeg);
    }

    protected override UWidget BP_GetDesiredFocusTarget()
    {
        return BTN_Neg;
    }

    public void InitializeFrom(FText title, FOnPromptResponse? onConfirm, FOnPromptResponse? onBack)
    {
        Title.Text = title;
        
        if (onConfirm != null)
        {
            OnConfirm += onConfirm;
        }
        
        if (onBack != null)
        {
            OnBack += onBack;
        }
    }
    
    [UFunction]
    private void OnClickPos(UCommonButtonBase button)
    {
        OnConfirm.Invoke();
        Cleanup();
    }
    
    [UFunction]
    private void OnClickNeg(UCommonButtonBase button)
    {
        OnBack.Invoke();
        Cleanup();
    }
    
    private void Cleanup()
    {
        OnConfirm.Clear();
        OnBack.Clear();
        DeactivateWidget();
    }
}