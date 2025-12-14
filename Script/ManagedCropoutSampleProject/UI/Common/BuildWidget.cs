using ManagedCropoutSampleProject.Core;
using ManagedCropoutSampleProject.Interactable;
using ManagedCropoutSampleProject.UI.Elements;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CommonUI;
using UnrealSharp.Core.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.SlateCore;
using UnrealSharp.UMG;

namespace ManagedCropoutSampleProject.UI.Common;

[UClass]
public partial class UBuildWidget : UCommonActivatableWidget
{
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected partial UCropoutButton BTN_Back { get; set; }
    
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected partial UHorizontalBox BuildItemsContainer { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected partial UDataTable BuildItemsDataTable { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected partial TSubclassOf<UBuildItemWidget> BuildItemWidgetClass { get; set; }

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

    public override void OnActivated()
    {
        WidgetLibrary.SetInputModeUIOnly(OwningPlayerController, null, EMouseLockMode.DoNotLock, false);
        BP_GetDesiredFocusTarget().SetFocus();

        ACropoutPlayer playerPawn = OwningPlayerPawnAs<ACropoutPlayer>();
        playerPawn.ActorTickEnabled = false;
        playerPawn.DisableInput(OwningPlayerController);
        
        playerPawn.SwitchBuildMode(true);
        
        base.OnActivated();
    }

    public override UWidget BP_GetDesiredFocusTarget()
    {
        return BTN_Back;
    }

    private void PopulateContainer()
    {
        if (!BuildItemsDataTable.IsValid())
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
        playerPawn.EnableInput(OwningPlayerController);
        playerPawn.SwitchBuildMode(false);
        DeactivateWidget();
    }
}