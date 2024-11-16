using UnrealSharp.Attributes;

namespace ManagedCropoutSampleProject.Core.Save;

[UInterface]
public interface IGameInstance
{
    [UFunction(FunctionFlags.BlueprintCallable)]
    public void UpdateAllVillagers();
}