using System.ComponentModel;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Core.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.DeveloperSettings;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Interactable;

[UClass(ClassFlags.Config | ClassFlags.DefaultConfig, Config = "Game")]
public partial class UInteractableSettings : UDeveloperSettings
{
    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.Config), Category("Visuals")]
    public partial TSoftObjectPtr<UTextureRenderTarget2D> RenderTarget { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.Config), Category("Visuals")]
    public partial TSoftObjectPtr<UMaterialInterface> DrawMaterial { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.Config), Category("Juice")]
    public partial TSoftObjectPtr<UCurveFloat> WobbleCurve { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.Config), Category("Juice")]
    public partial TSoftObjectPtr<UCurveFloat> CropPopCurve { get; set; }

    public Task LoadInteractableSettingsAsync()
    {
        List<FSoftObjectPath> objectsToLoad = new List<FSoftObjectPath>(2);
        objectsToLoad.Add(RenderTarget.SoftObjectPath);
        objectsToLoad.Add(DrawMaterial.SoftObjectPath);
        return objectsToLoad.LoadAsync();
    }
}