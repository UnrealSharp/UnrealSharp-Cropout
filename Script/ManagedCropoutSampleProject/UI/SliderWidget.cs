using ManagedCropoutSampleProject.Core.GameMode;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;
using UnrealSharp.Core;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI;

[UClass]
public partial class USliderWidget : UUserWidget
{
    [UProperty(PropertyFlags.EditAnywhere)]
    public partial USoundClass SoundClass { get; set; }
    
    [UProperty(PropertyFlags.EditAnywhere)]
    public partial FText SoundClassTitle { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial int Index { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected partial USoundMix SoundMix { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public partial UCommonTextBlock MixDescriptor { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    public partial USlider Slider { get; set; }

    protected override void PreConstruct_Implementation(bool isDesignTime)
    {
        base.PreConstruct_Implementation(isDesignTime);
        MixDescriptor.Text = SoundClassTitle;
    }

    protected override void Construct_Implementation()
    {
        Slider.OnValueChanged += OnSliderValueChanged;
        base.Construct_Implementation();
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