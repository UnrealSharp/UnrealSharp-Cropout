using ManagedCropoutSampleProject.Core;
using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.UI.Elements;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;
using UnrealSharp.Engine;
using UnrealSharp.SlateCore;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI.Common;

[UClass]
public class UBuildWidget : UCommonActivatableWidget
{
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected UCropoutButton BTN_Back { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected UHorizontalBox BuildItemsContainer { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected UDataTable BuildItemsDataTable { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected TSubclassOf<UBuildItemWidget> BuildItemWidgetClass { get; set; }

    public override void Construct()
    {
        BTN_Back.BindButtonClickedEvent(OnBack);
        base.Construct();
    }

    public override void PreConstruct(bool isDesignTime)
    {
        PopulateContainer();
        base.PreConstruct(isDesignTime);
    }

    protected override void OnActivated()
    {
        ACropoutPlayer playerPawn = OwningPlayerPawnAs<ACropoutPlayer>();
        playerPawn.ActorTickEnabled = false;
        playerPawn.DisableInput(OwningPlayerController);
        playerPawn.SwitchBuildMode(true);
        
        base.OnActivated();
    }

    private void PopulateContainer()
    {
        if (!BuildItemsDataTable.IsValid)
        {
            return;
        }
        
        BuildItemsContainer.ClearChildren();
        
        BuildItemsDataTable.ForEachRow<FResourceInfo>(info =>
        {
            UBuildItemWidget buildItemWidget = CreateWidget(BuildItemWidgetClass);
            buildItemWidget.InitializeFrom(info);
            UHorizontalBoxSlot slot = BuildItemsContainer.AddChildToHorizontalBox(buildItemWidget);
            slot.VerticalAlignment = EVerticalAlignment.VAlign_Bottom;
            FMargin padding = new FMargin();
            padding.Left = 8;
            padding.Right = 8;
            padding.Bottom = 5;
            slot.Padding = padding;
        });
    }
    
    [UFunction]
    private void OnBack(UCommonButtonBase buttonBase)
    {
        ACropoutPlayer playerPawn = OwningPlayerPawnAs<ACropoutPlayer>();
        playerPawn.ActorTickEnabled = true;
        playerPawn.EnableInput(OwningPlayerController);
        playerPawn.SwitchBuildMode(false);
        
        WidgetLibrary.SetInputMode_GameOnly(OwningPlayerController);
        
        DeactivateWidget();
    }
}