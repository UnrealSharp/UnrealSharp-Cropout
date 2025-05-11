using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI;

[UStruct]
public struct FJobs
{
    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadOnly)]
    public TSoftObjectPtr<UBehaviorTree> BehaviorTree;

    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadOnly)]
    public TSoftObjectPtr<UAnimMontage> WorkAnim;

    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadOnly)]
    public TSoftObjectPtr<USkeletalMesh> Hat;

    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadOnly)]
    public TSoftObjectPtr<UStaticMesh> Tool;
}