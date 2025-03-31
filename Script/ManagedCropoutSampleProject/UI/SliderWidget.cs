using ManagedCropoutSampleProject.Core.GameMode;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI;

[UClass]
public class USliderWidget : UUserWidget
{
    [UProperty(PropertyFlags.EditAnywhere)]
    public USoundClass SoundClass { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public FText SoundClassTitle { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public int Index { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected USoundMix SoundMix { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public UCommonTextBlock MixDescriptor { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public USlider Slider { get; set; }

    public override void PreConstruct(bool isDesignTime)
    {
        base.PreConstruct(isDesignTime);
        MixDescriptor.Text = SoundClassTitle;
    }

    public override void Construct()
    {
        Slider.OnValueChanged += OnSliderValueChanged;
        base.Construct();
    }
    
    [UFunction]
    private void OnSliderValueChanged(float value)
    {
        UCropoutGameInstance gameInstance = (UCropoutGameInstance) GameInstance;
        gameInstance.SoundMixes[Index] = value;
        UGameplayStatics.SetSoundMixClassOverride(SoundMix, SoundClass, gameInstance.SoundMixes[Index]);
    }

    public void UpdateSlider()
    {
        UCropoutGameInstance gameInstance = (UCropoutGameInstance) GameInstance;
        Slider.Value = gameInstance.SoundMixes[Index];
    }
}