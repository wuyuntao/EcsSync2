using MessagePack;

namespace EcsSync2
{
	public abstract class Event : Message
	{
		[Key( 0 )]
		public uint Time;
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
