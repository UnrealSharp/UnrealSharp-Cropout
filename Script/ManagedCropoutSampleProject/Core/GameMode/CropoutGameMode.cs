using ManagedCropoutSampleProject.Interactable;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedCropoutSampleProject.Core.GameMode;

[UMultiDelegate]
public delegate void FOnResourceChanged(EResourceType resourceType, int amount);

[UMultiDelegate]
public delegate void FOnUpdateVillagers(int villagerCount);

[UClass]
public class ACropoutGameMode : AGameModeBase, IResourceInterface
{
    [UProperty(PropertyFlags.BlueprintAssignable)]
    public TMulticastDelegate<FOnResourceChanged> OnResourceChanged { get; set; }
    
    [UProperty(PropertyFlags.BlueprintAssignable)]
    public TMulticastDelegate<FOnUpdateVillagers> OnUpdateVillagers { get; set; }
    
    [UProperty(PropertyFlags.EditDefaultsOnly)]
    protected TMap<EResourceType, int> Resources { get; set; }
    
    protected override void BeginPlay()
    {
        base.BeginPlay();
    }

    private void CreateGameHUD()
    {
        
    }

    public void RemoveResource(KeyValuePair<EResourceType, int> resource)
    {
        throw new NotImplementedException();
    }

    public void AddResource(KeyValuePair<EResourceType, int> resource)
    {
        throw new NotImplementedException();
    }

    public IDictionary<EResourceType, int> GetCurrentResources()
    {
        throw new NotImplementedException();
    }

    public void RemoveTargetResource(KeyValuePair<EResourceType, int> resource)
    {
        throw new NotImplementedException();
    }

    public bool CheckResource(EResourceType resourceType, out int amount)
    {
        if (Resources.TryGetValue(resourceType, out amount))
        {
            return true;
        }
        
        amount = 0;
        return false;
    }
}