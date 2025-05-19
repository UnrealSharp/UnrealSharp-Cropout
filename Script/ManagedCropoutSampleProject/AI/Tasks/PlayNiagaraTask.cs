using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.Niagara;

namespace ManagedCropoutSampleProject.AI.Tasks;

[UClass]
public class UPlayNiagaraTask : UCropoutBaseTask
{
    [UProperty(PropertyFlags.EditAnywhere)]
    public UNiagaraSystem NiagaraSystem { get; set; }
    
    protected override void ReceiveExecuteAI(AAIController ownerController, APawn controlledPawn)
    {
        UNiagaraFunctionLibrary.SpawnSystemAttached(NiagaraSystem, 
            controlledPawn.RootComponent, 
            FName.None, 
            FVector.Zero, 
            FRotator.ZeroRotator, 
            EAttachLocation.KeepRelativeOffset, 
            false);
        
        FinishExecute(true);
    }
}