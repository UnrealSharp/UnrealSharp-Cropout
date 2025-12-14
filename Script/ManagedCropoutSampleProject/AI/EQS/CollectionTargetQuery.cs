using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.Core;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI.EQS;

[UClass]
public partial class UCollectionTargetQuery : UEnvQueryContext_BlueprintBase
{
    public UCollectionTargetQuery()
    {
        KeyName = "Target";
    }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public partial FName KeyName { get; set; }

    public override void ProvideSingleLocation(UObject querierObject, AActor querierActor, out FVector resultingLocation)
    {
        AAIController controller = AIHelperLibrary.GetAIController(querierActor);
        UBlackboardComponent blackboard = AIHelperLibrary.GetBlackboard(controller);
        AActor actor = (AActor) blackboard.GetValueAsObject(KeyName);
        resultingLocation = actor.ActorLocation;
    }
}