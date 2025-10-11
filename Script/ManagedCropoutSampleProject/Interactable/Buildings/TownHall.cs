using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Interactable.Buildings;

[UClass]
public partial class ATownHall : ABuilding
{
    protected override void BeginPlay_Implementation()
    {
        APlayerController playerController = UGameplayStatics.GetPlayerController(0);
        playerController.ControlledPawn.SetActorLocation(ActorLocation, false, out _, false);
        base.BeginPlay_Implementation();
    }
}