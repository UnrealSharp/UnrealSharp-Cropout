using System.ComponentModel;
using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.NavigationSystem;

namespace ManagedCropoutSampleProject.AI.Tasks;

[UClass]
public class UFindClosestResource : UCropoutBaseTask
{
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public FBlackboardKeySelector Target { get; set; }

    [UProperty(PropertyFlags.EditInstanceOnly)]
    public FBlackboardKeySelector NearestTo { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly)]
    public FBlackboardKeySelector TargetClass { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly, Category = "Search Settings")]
    public bool UseBlackBoardClass { get; set; } = true;
    
    [UProperty(PropertyFlags.EditInstanceOnly, Category = "Search Settings"), UMetaData("EditCondition", "UseBlackBoardClass")]
    public TSubclassOf<AActor> ManualClass { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly, Category = "Tag")]
    public FName TagFiler { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly, Category = "Tag")]
    public FBlackboardKeySelector BlackBoardTag { get; set; }
    
    [UProperty(PropertyFlags.EditInstanceOnly, Category = "Tag")]
    bool UseBlackBoardTag { get; set; } = true;
    
    protected override void ReceiveExecute(AActor ownerActor)
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