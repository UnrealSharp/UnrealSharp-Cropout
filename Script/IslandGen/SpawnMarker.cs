using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace IslandGen;

[UClass]
public partial class ASpawnMarker : AActor
{
    [UProperty(DefaultComponent = true, RootComponent = true)]
    public partial USceneComponent Scene { get; set; }
}