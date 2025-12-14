using UnrealSharp.Attributes;
using UnrealSharp.Core.Attributes;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI;

[UClass]
public partial class UTransitionWidget : UUserWidget
{
    [UProperty(PropertyFlags.Transient), BindWidgetAnim]
    public partial UWidgetAnimation FadeAnimation { get; set; }

    public void TransitionIn()
    {
        PlayAnimation(FadeAnimation);
    }
    
    public void TransitionOut()
    {
        PlayAnimation(FadeAnimation, 0.0f, 0, EUMGSequencePlayMode.Reverse);

        [UFunction]
        void OnTransitionInFinished()
        {
            RemoveFromParent();
        }
        
        SystemLibrary.Delay(FadeAnimation.EndTime, new FLatentActionInfo(OnTransitionInFinished));
    }
}