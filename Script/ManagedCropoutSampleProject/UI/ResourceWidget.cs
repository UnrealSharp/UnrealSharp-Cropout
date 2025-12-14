using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.Interactable;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CommonUI;
using UnrealSharp.Core.Attributes;
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

    public override void PreConstruct(bool isDesignTime)
    {
        TSoftObjectPtr<UTexture2D> icon = ResourceIcons[_resourceType];
        ResourceIcon.SetBrushFromLazyTexture(icon);
        base.PreConstruct(isDesignTime);
    }

    public override void Construct()
    {
        IResourceInterface resourceInterface = (IResourceInterface) World.GameMode;

        if (resourceInterface.CheckResource(_resourceType, out int newResourceValue))
        {
            _currentResourceValue = newResourceValue;
            ResourceValue.Text = _currentResourceValue.ToString();
        }
        
        base.Construct();
    }

    public override void OnInitialized()
    {
        ACropoutGameMode gameMode = World.GameModeAs<ACropoutGameMode>();
        gameMode.OnResourceChanged += OnResourceChanged;
        
        base.OnInitialized();
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