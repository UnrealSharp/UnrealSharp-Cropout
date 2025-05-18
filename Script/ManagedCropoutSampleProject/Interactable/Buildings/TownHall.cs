using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Interactable.Buildings;

[UClass]
public class ATownHall : ABuilding
{
    protected override void BeginPlay()
    {
        APlayerController playerController = UGameplayStatics.GetPlayerController(0);
        playerController.ControlledPawn.SetActorLocation(ActorLocation, false, out _, false);
        base.BeginPlay();
    }
}