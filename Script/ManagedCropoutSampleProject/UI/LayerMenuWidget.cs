using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Attributes.MetaTags;
using UnrealSharp.CommonUI;

namespace ManagedCropoutSampleProject.UI;

[UClass]
public class ULayerMenuWidget : UCommonActivatableWidget
{
    [UProperty(PropertyFlags.BlueprintReadOnly), BindWidget]
    protected UCommonActivatableWidgetStack MainStack { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected TSubclassOf<UMainMenuWidget> MainMenuWidgetClass { get; set; }

    protected override void OnActivated()
    {
        base.OnActivated();
        
        UMainMenuWidget widget = MainStack.PushWidget(MainMenuWidgetClass);
        widget.InitializeFrom(MainStack);
    }
}