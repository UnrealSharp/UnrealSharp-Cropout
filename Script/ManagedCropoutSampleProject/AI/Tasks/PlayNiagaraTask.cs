using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.Core;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.Niagara;

namespace ManagedCropoutSampleProject.AI.Tasks;

[UClass]
public partial class UPlayNiagaraTask : UCropoutBaseTask
{
    [UProperty(PropertyFlags.EditAnywhere)]
    public partial UNiagaraSystem NiagaraSystem { get; set; }
    
    public override void ReceiveExecuteAI(AAIController ownerController, APawn controlledPawn)
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