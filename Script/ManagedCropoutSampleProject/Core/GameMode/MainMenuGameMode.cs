using IslandGen;
using ManagedCropoutSampleProject.UI;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.AudioModulation;
using UnrealSharp.Core.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Core.GameMode;

[UClass]
public partial class AMainMenuGameMode : AGameModeBase
{
    [UProperty(PropertyFlags.EditDefaultsOnly), Category("Main Menu")]
    protected partial TSubclassOf<ULayerMenuWidget> MainMenuWidgetClass { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected partial UTextureRenderTarget2D RenderTarget { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly), Category("Music")]
    protected partial USoundControlBus PianoBus { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly), Category("Music")]
    protected partial USoundControlBus MusicPercBus { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly), Category("Music")]
    protected partial USoundControlBus StringsDelayBus { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly), Category("Music")]
    protected partial USoundControlBus MusicWinLoseBus { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly), Category("Music")]
    protected partial USoundBase MainMenuMusic { get; set; }

    public override void BeginPlay()
    {
        base.BeginPlay();
        
        UCropoutGameInstance gameInstance = (UCropoutGameInstance) World.GameInstance;
        gameInstance.Transition(ETransitionType.Out);
        RenderingLibrary.ClearRenderTarget2D(RenderTarget, FLinearColor.Black);

        ULayerMenuWidget mainMenuWidget = CreateWidget(MainMenuWidgetClass);
        mainMenuWidget.AddToViewport();
        mainMenuWidget.ActivateWidget();
        
        SetupMusicControls();
        IslandGen();
    }

    private void SetupMusicControls()
    {
        UAudioModulationStatics.SetGlobalControlBusMixValue(PianoBus, 0.0f, 1.0f);
        UAudioModulationStatics.SetGlobalControlBusMixValue(MusicPercBus, 0.0f, 1.0f);
        UAudioModulationStatics.SetGlobalControlBusMixValue(StringsDelayBus, 0.5f, 1.0f);
        UAudioModulationStatics.SetGlobalControlBusMixValue(MusicWinLoseBus, 0.5f, 10.0f);
        
        UCropoutGameInstance gameInstance = (UCropoutGameInstance) World.GameInstance;
        gameInstance.PlayMusic(MainMenuMusic, 1.0f, true);
    }

    private void IslandGen()
    {
        AIslandGenActor islandGenActor = UGameplayStatics.GetActorOfClass<AIslandGenActor>();
        ASpawner spawner = UGameplayStatics.GetActorOfClass<ASpawner>();
        
        islandGenActor.InitializeFromSeed(new FRandomStream(28139));
        spawner.SpawnRandom();
    }
}