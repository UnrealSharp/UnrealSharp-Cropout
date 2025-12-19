using System.ComponentModel;
using ManagedCropoutSampleProject.Core.GameMode;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Interactable;

[UClass]
public partial class ABuilding : AInteractable
{
    public ABuilding()
    {
        RequireBuild = true;
        BuildDifficulty = 1.0f;
    }
    
    [UProperty(PropertyFlags.EditDefaultsOnly), Category("Build")]
    public partial int CurrentStage { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    public partial float BuildDifficulty { get; set; }

    
    [UProperty(DefaultComponent = true, AttachmentComponent = "Scene")]
    public partial UBoxComponent NavBlocker { get; set; }

    public override float Interact()
    {
        base.Interact();
        return ProgressConstruct(0.4f);
    }

    public void SpawnInBuildMode(float progression)
    {
        ProgressionState = progression;
        Tags.Add("Build");
        
        int meshCount = MeshList.Count;
        float progressionState = progression * meshCount;
        int newStage = MathLibrary.Floor(progressionState);
        
        UStaticMesh newMesh = MeshList[newStage];
        
        if (newMesh.IsValid())
        {
            Mesh.SetStaticMesh(newMesh);
        }
    }

    public float ProgressConstruct(float investedTime)
    {
        ProgressionState += investedTime / BuildDifficulty;
        
        int newStage = MathLibrary.Floor(ProgressionState);
        int lastStage = MeshList.Count - 1;

        if (newStage >= lastStage)
        {
            ConstructionComplete();
            
            UStaticMesh newMesh = MeshList[lastStage];
            Mesh.SetStaticMesh(newMesh);
        }
        else
        {
            if (newStage != CurrentStage)
            {
                CurrentStage = newStage;
                UStaticMesh newMesh = MeshList[newStage];
                Mesh.SetStaticMesh(newMesh);
            }
        }
        
        UCropoutGameInstance gameInstance = World.GameInstanceAs<UCropoutGameInstance>();
        gameInstance.UpdateAllInteractables();
        
        float remainingTime = lastStage - ProgressionState;
        return remainingTime;
    }

    public virtual void ConstructionComplete()
    {
        Tags.Remove("Build");
        UCropoutGameInstance gameInstance = World.GameInstanceAs<UCropoutGameInstance>();
        gameInstance.UpdateAllInteractables();
    }
}