using ManagedCropoutSampleProject.Interactable;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Core.Save;

[UStruct]
public struct FInteractableSaveData
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
public struct FVillagerSaveData
{
    [UProperty(PropertyFlags.SaveGame)]
    public FVector Transform;

    [UProperty(PropertyFlags.SaveGame)] 
    public FName Task;
}

[UClass]
public class UCropoutSaveObject : USaveGame
{
    [UProperty(PropertyFlags.SaveGame)]
    public FRandomStream RandomStream { get; set; }
    
    [UProperty(PropertyFlags.SaveGame)]
    public TMap<EResourceType, int> Resources { get; set; }
    
    [UProperty(PropertyFlags.SaveGame)]
    public TArray<FInteractableSaveData> Interactables { get; set; }
    
    [UProperty(PropertyFlags.SaveGame)]
    public TArray<FVillagerSaveData> Villagers { get; set; }
    
    [UProperty(PropertyFlags.SaveGame)]
    public float PlayTime { get; set; }
    
    public void ClearSave()
    {
        Resources.Clear();
        Interactables.Clear();
        Villagers.Clear();
        PlayTime = 0.0f;
    }
}