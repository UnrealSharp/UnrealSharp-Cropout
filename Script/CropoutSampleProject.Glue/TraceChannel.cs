using UnrealSharp.Engine;

public enum ETraceChannel
{
	Visibility = 0,
	Camera = 1,
}

public static class TraceChannelStatics
{
	public static ETraceTypeQuery ToQuery(this ETraceChannel traceTypeQueryHelper)
	{
		return (ETraceTypeQuery)traceTypeQueryHelper;
	}
}

public enum ECollisionChannel
{
}

public static class CollisionChannelStatics
{
	public static EObjectTypeQuery ToObjectTypeQuery(this ECollisionChannel traceTypeQueryHelper)
	{
		return (EObjectTypeQuery)traceTypeQueryHelper;
	}
}