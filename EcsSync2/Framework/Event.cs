using MessagePack;

namespace EcsSync2
{
	[MessagePackObject]
	public abstract class Event : Message
	{
		[Key( 0 )]
		public uint Time;

		public override string ToString()
		{
			return $"{GetType().Name} #{Time}";
		}
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
