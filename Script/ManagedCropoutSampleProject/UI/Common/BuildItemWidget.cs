using System.Diagnostics;
using ManagedCropoutSampleProject.Core;
using ManagedCropoutSampleProject.Core.GameMode;
using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.UI.Elements;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;
using UnrealSharp.Core;
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
    
    [UProperty]
    private TSubclassOf<AInteractable> InteractableClass { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    private FResourceInfo TableData { get; set; }

    private bool _enableBuilding = true;
    
    public void InitializeFrom(FResourceInfo inTableData)
    {
        TableData = inTableData;
        _enableBuilding = true;

        UpdateVisuals();
        PopulateCost();
        CheckIfItemEnabled();
    }

    public override void Construct()
    {
        ACropoutGameMode gameMode = World.GameModeAs<ACropoutGameMode>();
        gameMode.OnResourceChanged += OnResourceChanged;
        CheckIfItemEnabled();
        
        base.Construct();
    }

    protected override void BP_OnClicked()
    {
        IPlayer pawnInterface = (IPlayer) OwningPlayerPawn;
        
        if (pawnInterface != null)
        {
            pawnInterface.BeginBuild(InteractableClass, TableData.Cost);
        }

        ACropoutGameMode player = World.GameModeAs<ACropoutGameMode>();
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

    async void UpdateVisuals()
    {
        try
        {
            Txt_Title.Text = TableData.Title;
            Img_Icon.SetBrushFromSoftTexture(TableData.UIIcon);
        
            InteractableClass = await TableData.TargetClass.LoadAsync();
            BG.BrushColor = MathLibrary.Conv_ColorToLinearColor(TableData.TabColor);
        }
        catch (Exception e)
        {
            LogCropout.Log(e.Message);
        }
    }

    void PopulateCost()
    {
        CostContainer.ClearChildren();
        
        foreach (KeyValuePair<EResourceType, int> cost in TableData.Cost)
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
        CheckIfItemEnabled();
    }

    void CheckIfItemEnabled()
    {
        ACropoutGameMode gameMode = World.GameModeAs<ACropoutGameMode>();
        
        if (!gameMode)
        {
            return;
        }
        
        _enableBuilding = true;
        foreach (KeyValuePair<EResourceType, int> costResource in TableData.Cost)
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