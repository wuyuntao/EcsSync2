using MessagePack;

namespace EcsSync2
{
	[MessagePackObject]
	public abstract class Event : Referencable
	{
	}

	public abstract class SceneEvent : Event
	{
	}

	[MessagePackObject]
	public class ComponentEvent : Event
	{
		[Key( 10 )]
		public uint ComponentId;
	}
}
