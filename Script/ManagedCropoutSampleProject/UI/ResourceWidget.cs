using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.Interactable;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI;

[UClass]
public partial class UResourceWidget : UUserWidget
{
    [UProperty, BindWidget]
    protected partial UCommonTextBlock ResourceValue { get; set; }
    
    [UProperty, BindWidget]
    protected partial UCommonLazyImage ResourceIcon { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected partial TMap<EResourceType, TSoftObjectPtr<UTexture2D>> ResourceIcons { get; set; }
    
    [UProperty(PropertyFlags.Transient), BindWidgetAnim]
    public partial UWidgetAnimation Reduce { get; set; }
    
    [UProperty(PropertyFlags.Transient), BindWidgetAnim]
    public partial UWidgetAnimation Increase { get; set; }

    private int _currentResourceValue = 0;
    private EResourceType _resourceType;
    
    public void InitializeFrom(EResourceType inResourceType)
    {
        _resourceType = inResourceType;
        ResourceValue.Text = "0";
    }

    protected override void PreConstruct_Implementation(bool isDesignTime)
    {
        TSoftObjectPtr<UTexture2D> icon = ResourceIcons[_resourceType];
        ResourceIcon.SetBrushFromLazyTexture(icon);
        base.PreConstruct_Implementation(isDesignTime);
    }

    protected override void Construct_Implementation()
    {
        IResourceInterface resourceInterface = (IResourceInterface) World.GameMode;

        if (resourceInterface.CheckResource(_resourceType, out int newResourceValue))
        {
            _currentResourceValue = newResourceValue;
            ResourceValue.Text = _currentResourceValue.ToString();
        }
        
        base.Construct_Implementation();
    }

    protected override void OnInitialized_Implementation()
    {
        ACropoutGameMode gameMode = World.GameModeAs<ACropoutGameMode>();
        gameMode.OnResourceChanged += OnResourceChanged;
        
        base.OnInitialized_Implementation();
    }
    
    [UFunction]
    public void OnResourceChanged(EResourceType type, int value)
    {
        if (_resourceType != type)
        {
            return;
        }
        
        UWidgetAnimation animation = value > _currentResourceValue ? Increase : Reduce;
        PlayAnimation(animation);
        _currentResourceValue = value;
        ResourceValue.Text = _currentResourceValue.ToString();
    }
}