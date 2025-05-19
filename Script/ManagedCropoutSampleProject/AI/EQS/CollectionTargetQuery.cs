using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI.EQS;

[UClass]
public class UCollectionTargetQuery : UEnvQueryContext_BlueprintBase
{
    public UCollectionTargetQuery()
    {
        KeyName = "Target";
    }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public FName KeyName { get; set; }
    
    public override void ProvideSingleLocation(UObject querierObject, AActor querierActor, out FVector resultingLocation)
    {
        AAIController controller = AIHelperLibrary.GetAIController(querierActor);
        UBlackboardComponent blackboard = AIHelperLibrary.GetBlackboard(controller);
        AActor actor = (AActor) blackboard.GetValueAsObject(KeyName);
        resultingLocation = actor.ActorLocation;
    }
}