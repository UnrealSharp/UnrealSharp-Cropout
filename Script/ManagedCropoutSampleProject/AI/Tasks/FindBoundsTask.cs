using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI.Tasks;

[UClass]
public class UFindBoundsTask : UCropoutBaseTask
{
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public FBlackboardKeySelector Target { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public float AdditionalBounds { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public FBlackboardKeySelector BlackBoardBounds { get; set; }
    
    protected override void ReceiveExecute(AActor ownerActor)
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