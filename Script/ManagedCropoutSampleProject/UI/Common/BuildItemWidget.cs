using ManagedCropoutSampleProject.Core;
using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.UI.Elements;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI.Common;

[UClass]
public class UBuildItemWidget : UCommonButtonBase
{
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public USizeBox BaseSize { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public UImage Img_Icon { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public UBorder BG { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public UCommonTextBlock Txt_Title { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), UMetaData("BindWidget")]
    public UHorizontalBox CostContainer { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public TSubclassOf<UBuildItemCostWidget> CostWidgetClass { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public TSubclassOf<UCommonActivatableWidget> ConfirmWidgetClass { get; set; }
    
    [UProperty(PropertyFlags.Transient), BindWidgetAnim]
    public UWidgetAnimation Loop_Hover { get; set; }
    
    [UProperty(PropertyFlags.Transient), BindWidgetAnim]
    public UWidgetAnimation Highlight_In { get; set; }

    private FResourceInfo _tableData;
    private TSubclassOf<AInteractable> _hardClassRef;
    private bool _enableBuilding = true;
    
    
    public void InitializeFrom(FResourceInfo inTableData)
    {
        _tableData = inTableData;
        _enableBuilding = true;
        
        UpdateVisuals();
        PopulateCost();
    }

    public override void Construct()
    {
        ACropoutGameMode gameMode = World.GameModeAs<ACropoutGameMode>();
        gameMode.OnResourceChanged += OnResourceChanged;
        base.Construct();
    }

    protected override void BP_OnClicked()
    {
        IPlayer pawnInterface = (IPlayer) OwningPlayerPawn;
        
        if (pawnInterface != null)
        {
            pawnInterface.BeginBuild(_hardClassRef, _tableData.Cost);
        }

        IPlayer player = World.GameMode as IPlayer;
        player.AddUI(ConfirmWidgetClass);
    }

    protected override void BP_OnHovered()
    {
        BaseSize.MinDesiredHeight = 300.0f;
        PlayAnimation(Loop_Hover);
        PlayAnimation(Highlight_In);
        
        base.BP_OnHovered();
    }

    protected override void BP_OnUnhovered()
    {
        BaseSize.MinDesiredHeight = 250.0f;
        StopAnimation(Loop_Hover);
        StopAnimation(Highlight_In);
        base.BP_OnUnhovered();
    }

    void UpdateVisuals()
    {
        Txt_Title.Text = _tableData.Title;
        Img_Icon.SetBrushFromSoftTexture(_tableData.UIIcon);
        
        _tableData.TargetClass.LoadAsync([UFunction](asset) =>
        {
            _hardClassRef = new TSubclassOf<AInteractable>((UClass) asset);
        });
        
        BG.BrushColor = MathLibrary.Conv_ColorToLinearColor(_tableData.TabColor);
    }

    void PopulateCost()
    {
        CostContainer.ClearChildren();
        
        foreach (KeyValuePair<EResourceType, int> cost in _tableData.Cost)
        {
            UBuildItemCostWidget costWidget = CreateWidget(CostWidgetClass);
            costWidget.InitializeFromResourceCost(cost);
            UHorizontalBoxSlot slot = CostContainer.AddChildToHorizontalBox(costWidget);
            slot.Size = new FSlateChildSize
            {
                Value = 1,
                SizeRule = ESlateSizeRule.Fill
            };
        }
    }

    [UFunction]
    public void OnResourceChanged(EResourceType resourceType, int amount)
    {
        ACropoutGameMode gameMode = World.GameModeAs<ACropoutGameMode>();
        _enableBuilding = true;
        foreach (KeyValuePair<EResourceType, int> costResource in _tableData.Cost)
        {
            gameMode.CheckResource(costResource.Key, out int newResourceValue);
            
            if (newResourceValue <= costResource.Value)
            {
                _enableBuilding = false;
                break;
            }
        }
        
        SetIsInteractionEnabled(_enableBuilding);
    }
}