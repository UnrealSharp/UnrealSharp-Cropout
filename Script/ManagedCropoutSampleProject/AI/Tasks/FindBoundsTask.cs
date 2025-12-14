using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI.Tasks;

[UClass]
public partial class UFindBoundsTask : UCropoutBaseTask
{
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector Target { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial float AdditionalBounds { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector BlackBoardBounds { get; set; }
    
    public override void ReceiveExecute(AActor ownerActor)
    {
        AActor target = UBTFunctionLibrary.GetBlackboardValueAsActor(this, Target);

        if (target == null)
        {
            FinishExecute(false);
            return;
        }
        
        target.GetActorBounds(true, out FVector origin, out FVector boxExtent);

        double bounds = double.Min(boxExtent.X, boxExtent.Y) + AdditionalBounds;
        
        UBTFunctionLibrary.SetBlackboardValueAsFloat(this, BlackBoardBounds, (float)bounds);
        FinishExecute(true);
    }
}