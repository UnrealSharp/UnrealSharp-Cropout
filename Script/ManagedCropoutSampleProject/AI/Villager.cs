using UnrealSharp.Attributes;
using UnrealSharp.Core;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.AI;

[UInterface]
public interface IVillager
{
    [UFunction(FunctionFlags.BlueprintCallable)]
    public void Action(AActor? actor);

    [UFunction(FunctionFlags.BlueprintCallable)]
    public void ChangeJob(FName newJob);

    [UFunction(FunctionFlags.BlueprintCallable)]
    public float PlayDeliverAnim();

    [UFunction(FunctionFlags.BlueprintCallable)]
    public void PlayWorkAnim(float delay);

    [UFunction(FunctionFlags.BlueprintCallable)]
    public float ProgressBuilding(float timeRemaining);

    [UFunction(FunctionFlags.BlueprintCallable)]
    public void ReturnToDefaultBT();
}