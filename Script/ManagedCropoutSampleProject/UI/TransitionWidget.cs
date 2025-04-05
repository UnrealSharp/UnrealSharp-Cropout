using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI;

[UClass]
public class UTransitionWidget : UUserWidget
{
    [UProperty(PropertyFlags.Transient), BindWidgetAnim]
    public UWidgetAnimation FadeAnimation { get; set; }

    public void TransitionIn()
    {
        PlayAnimation(FadeAnimation);
    }
    
    public void TransitionOut()
    {
        PlayAnimation(FadeAnimation, 0.0f, 0, EUMGSequencePlayMode.Reverse);

        FLatentActionInfo latentInfo = new FLatentActionInfo();
        latentInfo.CallbackTarget = this;
        latentInfo.ExecutionFunction = nameof(OnTransitionInFinished);
        SystemLibrary.Delay(FadeAnimation.EndTime, latentInfo);
    }
    
    [UFunction]
    public void OnTransitionInFinished()
    {
        RemoveFromParent();
    }
}