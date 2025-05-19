using ManagedCropoutSampleProject.Interactable.Buildings;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI.EQS;

[UClass]
public class UTargetTownHallQuery : UEnvQueryContext_BlueprintBase
{
    public override void ProvideSingleLocation(UObject querierObject, AActor querierActor, out FVector resultingLocation)
    {
        ATownHall townHall = UGameplayStatics.GetActorOfClass<ATownHall>();
        
        if (townHall == null)
        {
            resultingLocation = FVector.Zero;
            return;
        }

        resultingLocation = townHall.ActorLocation;
    }
}