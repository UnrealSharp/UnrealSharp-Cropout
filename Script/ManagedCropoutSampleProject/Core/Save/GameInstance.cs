using UnrealSharp.Attributes;

namespace ManagedCropoutSampleProject.Core.Save;

[UInterface]
public partial interface IGameInstance
{
    [UFunction(FunctionFlags.BlueprintCallable)]
    public void UpdateAllVillagers();
    
    [UFunction(FunctionFlags.BlueprintCallable)]
    public void UpdateAllInteractables();

    [UFunction(FunctionFlags.BlueprintCallable)]
    public void SaveGame();

    [UFunction(FunctionFlags.BlueprintCallable)]
    public void ClearSave(bool clearSeed);

    [UFunction(FunctionFlags.BlueprintCallable)]
    public void LoadLevel();
}