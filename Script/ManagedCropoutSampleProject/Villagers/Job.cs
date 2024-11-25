using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Villagers;

[UStruct]
public struct FJob
{
    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadWrite)]
    public TSoftObjectPtr<UBehaviorTree> BehaviorTree;
    
    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadWrite)]
    public TSoftObjectPtr<UAnimMontage> AnimMontage;

    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadWrite)]
    public TSoftObjectPtr<USkeletalMesh> Hat;

    [UProperty(PropertyFlags.EditDefaultsOnly | PropertyFlags.BlueprintReadWrite)]
    public TSoftObjectPtr<UStaticMesh> Tool;
}