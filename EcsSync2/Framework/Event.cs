using MessagePack;

namespace EcsSync2
{
	public abstract class Event : Message
	{
	}

	public abstract class SceneEvent : Event
	{
	}

	[MessagePackObject]
	public class ComponentEvent : Event
	{
		[Key( 0 )]
		public uint ComponentId;
	}
}
