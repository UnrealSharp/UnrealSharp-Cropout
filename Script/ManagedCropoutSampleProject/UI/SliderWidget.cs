using ManagedCropoutSampleProject.Core.GameMode;
using UnrealSharp.Attributes;
using UnrealSharp.CommonUI;
using UnrealSharp.Core;
using UnrealSharp.Core.Attributes;
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