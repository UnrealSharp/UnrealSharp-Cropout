using ManagedCropoutSampleProject.Interactable;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Core;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Core.Save;

[UStruct]
public partial struct FInteractableSaveData
{
    [UProperty(PropertyFlags.SaveGame)]
    public FTransform Transform;

    [UProperty(PropertyFlags.SaveGame)]
    public TSubclassOf<AInteractable> Type;
    
    [UProperty(PropertyFlags.SaveGame)]
    public float Health;

    [UProperty(PropertyFlags.SaveGame)] 
    public FName Tag;
}

[UStruct]
public partial struct FVillagerSaveData
{
    [UProperty(PropertyFlags.SaveGame)]
    public FVector Transform;

    [UProperty(PropertyFlags.SaveGame)] 
    public FName Task;
}

[UClass]
public partial class UCropoutSaveObject : USaveGame
{
    [UProperty(PropertyFlags.SaveGame)]
    public partial FRandomStream RandomStream { get; set; }
    
    [UProperty(PropertyFlags.SaveGame)]
    public partial TMap<EResourceType, int> Resources { get; set; }
    
    [UProperty(PropertyFlags.SaveGame)]
    public partial TArray<FInteractableSaveData> Interactables { get; set; }
    
    [UProperty(PropertyFlags.SaveGame)]
    public partial TArray<FVillagerSaveData> Villagers { get; set; }
    
    [UProperty(PropertyFlags.SaveGame)]
    public partial float PlayTime { get; set; }
    
    public void ClearSave()
    {
        Resources.Clear();
        Interactables.Clear();
        Villagers.Clear();
        PlayTime = 0.0f;
    }
}