using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI;

[UClass]
public class UTransitionWidget : UUserWidget
{
    [UProperty(PropertyFlags.Transient), UMetaData("BindWidgetAnim")]
    public UWidgetAnimation FadeAnimation { get; set; }

    public void TransitionIn()
    {
        PlayAnimation(FadeAnimation);
    }
    
    public async void TransitionOut()
    {
        try
        {
            PlayAnimation(FadeAnimation, 0.0f, 0, EUMGSequencePlayMode.Reverse);
        
            int delayMs = (int) (FadeAnimation.EndTime * 1000);
            await Task.Delay(delayMs).ConfigureWithUnrealContext();
        
            RemoveFromParent();
        }
        catch (Exception e)
        {
            LogCropout.LogError(e.Message);
        }
    }
}