using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;

namespace ManagedCropoutSampleProject.UI;

[UClass]
public partial class ULayerMenuWidget : UCommonActivatableWidget
{
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected partial UCommonActivatableWidgetStack MainStack { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected partial TSubclassOf<UMainMenuWidget> MainMenuWidgetClass { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected partial TSubclassOf<UMainMenuWidget> MainMenuWidgethhasfssssasfaClass { get; set; }

    protected override void OnActivated_Implementation()
    {
        base.OnActivated_Implementation();
        
        UMainMenuWidget widget = MainStack.PushWidget(MainMenuWidgetClass);
        widget.InitializeFrom(MainStack);
    }
}