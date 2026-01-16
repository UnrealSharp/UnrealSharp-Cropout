using UnrealSharp.Engine;
using UnrealSharp.Core;

public enum ETraceChannel
{
	Visibility = 0,
	Camera = 1,
}

public enum EObjectChannel
{
}

public static class CollisionProfiles
{
	public static readonly FName NoCollision = new("NoCollision");
	public static readonly FName BlockAll = new("BlockAll");
	public static readonly FName OverlapAll = new("OverlapAll");
	public static readonly FName BlockAllDynamic = new("BlockAllDynamic");
	public static readonly FName OverlapAllDynamic = new("OverlapAllDynamic");
	public static readonly FName IgnoreOnlyPawn = new("IgnoreOnlyPawn");
	public static readonly FName OverlapOnlyPawn = new("OverlapOnlyPawn");
	public static readonly FName Pawn = new("Pawn");
	public static readonly FName Spectator = new("Spectator");
	public static readonly FName CharacterMesh = new("CharacterMesh");
	public static readonly FName PhysicsActor = new("PhysicsActor");
	public static readonly FName Destructible = new("Destructible");
	public static readonly FName InvisibleWall = new("InvisibleWall");
	public static readonly FName InvisibleWallDynamic = new("InvisibleWallDynamic");
	public static readonly FName Trigger = new("Trigger");
	public static readonly FName Ragdoll = new("Ragdoll");
	public static readonly FName Vehicle = new("Vehicle");
	public static readonly FName UI = new("UI");
	public static readonly FName WaterBodyCollision = new("WaterBodyCollision");
}

public static class QueryChannelConverter
{
	public static ETraceTypeQuery ToTraceQuery(this ETraceChannel traceChannel) => (ETraceTypeQuery)traceChannel;
	public static ETraceChannel ToTraceChannel(this ETraceTypeQuery traceTypeQuery) => (ETraceChannel)traceTypeQuery;
	public static EObjectTypeQuery ToObjectQuery(this EObjectChannel objectChannel) => (EObjectTypeQuery)objectChannel;
	public static EObjectChannel ToObjectChannel(this EObjectTypeQuery objectTypeQuery) => (EObjectChannel)objectTypeQuery;
}