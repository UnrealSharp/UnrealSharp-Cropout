using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace IslandGen;

[UClass]
public class ASpawnMarker : AActor
{
    [UProperty(DefaultComponent = true, RootComponent = true)]
    public USceneComponent Scene { get; set; }
}