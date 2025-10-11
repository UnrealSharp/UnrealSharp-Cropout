using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.Core;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.NavigationSystem;

namespace ManagedCropoutSampleProject.AI.Tasks;

[UClass]
public partial class UFindClosestResource : UCropoutBaseTask
{
    public UFindClosestResource()
    {
        UseBlackBoardClass = true;
        UseBlackBoardTag = true;
    }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector Target { get; set; }

    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector NearestTo { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public partial FBlackboardKeySelector TargetClass { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly, Category = "Search Settings")]
    public partial bool UseBlackBoardClass { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly, Category = "Search Settings"), UMetaData("EditCondition", "UseBlackBoardClass")]
    public partial TSubclassOf<AActor> ManualClass { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly, Category = "Tag")]
    public partial FName TagFiler { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly, Category = "Tag")]
    public partial FBlackboardKeySelector BlackBoardTag { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly, Category = "Tag")]
    partial bool UseBlackBoardTag { get; set; }
    
    protected override void ReceiveExecute_Implementation(AActor ownerActor)
    {
        AActor foundActor = UBTFunctionLibrary.GetBlackboardValueAsActor(this, Target);

        if (foundActor != null)
        {
            FinishExecute(true);
            return;
        }

        TSubclassOf<AActor> classToSearchFor;
        if (UseBlackBoardClass)
        {
            TSubclassOf<UObject> foundClass = UBTFunctionLibrary.GetBlackboardValueAsClass(this, TargetClass);

            if (!foundClass.Valid)
            {
                FinishExecute(false);
                return;
            }
            
            classToSearchFor = foundClass.As<AActor>();
        }
        else
        {
            classToSearchFor = ManualClass;
        }

        FName tag;
        if (UseBlackBoardTag)
        {
            tag = UBTFunctionLibrary.GetBlackboardValueAsName(this, BlackBoardTag);
        }
        else
        {
            tag = TagFiler;
        }

        IList<AActor> foundActors;
        if (tag.IsNone)
        {
            UGameplayStatics.GetAllActorsOfClass(classToSearchFor, out foundActors);
        }
        else
        {
            UGameplayStatics.GetAllActorsOfClassWithTag(classToSearchFor, tag, out foundActors);
        }
        
        if (foundActors.Count == 0)
        {
            FinishExecute(false);
            return;
        }
        
        bool pathFound = false;

        AActor nearestActor = UBTFunctionLibrary.GetBlackboardValueAsActor(this, NearestTo);
        FVector nearestLocation = nearestActor.ActorLocation;
        
        for (int i = foundActors.Count - 1; i >= 0; i--)
        {
            AActor nearestActorItr = UGameplayStatics.FindNearestActor(nearestLocation, foundActors, out _);
            UNavigationPath navigationPath = UNavigationSystemV1.FindPathToActorSynchronously(nearestLocation, nearestActorItr, 100);

            if (navigationPath.IsPartial())
            {
                foundActors.RemoveAt(i);
            }
            else
            {
                pathFound = true;
                UBTFunctionLibrary.SetBlackboardValueAsObject(this, Target, nearestActorItr);
                break;
            }
        }
        
        FinishExecute(pathFound);
    }
}