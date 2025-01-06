using ManagedCropoutSampleProject.Core;
using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.UI.Elements;
using UnrealSharp;
using UnrealSharp.Attributes;
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

    private FResourceInfo _tableData;
    private TSubclassOf<AInteractable> _hardClassRef;

    public override void PreConstruct(bool isDesignTime)
    {
        base.PreConstruct(isDesignTime);
        UpdateVisuals();
        PopulateCost();
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

    void UpdateVisuals()
    {
        Txt_Title.Text = _tableData.Title;
        Img_Icon.SetBrushFromSoftTexture(_tableData.UIIcon);
        
        _tableData.TargetClass.LoadAsync(asset =>
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
            UBuildItemCostWidget costWidget = CreateWidget<UBuildItemCostWidget>(CostWidgetClass);
            costWidget.InitializeFromResourceCost(cost);
            UHorizontalBoxSlot slot = CostContainer.AddChildToHorizontalBox(costWidget);
            slot.Size = new FSlateChildSize
            {
                Value = 1,
                SizeRule = ESlateSizeRule.Fill
            };
        }
    }
}