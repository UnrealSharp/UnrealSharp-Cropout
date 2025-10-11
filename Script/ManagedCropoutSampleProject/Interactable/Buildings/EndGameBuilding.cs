using ManagedCropoutSampleProject.Core.GameMode;
using UnrealSharp.Attributes;

namespace ManagedCropoutSampleProject.Interactable.Buildings;

[UClass]
public partial class AEndGameBuilding : ABuilding
{
    public override void ConstructionComplete()
    {
        base.ConstructionComplete();
        ACropoutGameMode gameMode = World.GameModeAs<ACropoutGameMode>();
        gameMode.EndGame(true);
    }
}