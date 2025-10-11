using ManagedCropoutSampleProject.UI.Elements;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;
using UnrealSharp.Core;
using UnrealSharp.UMG;
using UnrealSharp.UnrealSharpCore;

namespace ManagedCropoutSampleProject.UI;

[UMultiDelegate]
public delegate void FOnPromptResponse();

[UClass]
public partial class UPromptWidget : UCommonActivatableWidget
{
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected partial UCommonTextBlock Title { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected partial UCropoutButton BTN_Pos { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected partial UCropoutButton BTN_Neg { get; set; }
    
    [UProperty(PropertyFlags.BlueprintAssignable)]
    protected partial TMulticastDelegate<FOnPromptResponse> OnConfirm { get; set; }
    
    [UProperty(PropertyFlags.BlueprintAssignable)]
    protected partial TMulticastDelegate<FOnPromptResponse> OnBack { get; set; }

    protected override void Construct_Implementation()
    {
        base.Construct_Implementation();
        
        BTN_Pos.BindButtonClickedEvent(OnClickPos);
        BTN_Neg.BindButtonClickedEvent(OnClickNeg);
    }

    protected override UWidget BP_GetDesiredFocusTarget_Implementation()
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