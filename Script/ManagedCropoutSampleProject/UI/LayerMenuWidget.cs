using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CommonUI;
using UnrealSharp.Core.Attributes;

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

    public override void OnActivated()
    {
        base.OnActivated();
        
        UMainMenuWidget widget = MainStack.PushWidget(MainMenuWidgetClass);
        widget.InitializeFrom(MainStack);
    }
}