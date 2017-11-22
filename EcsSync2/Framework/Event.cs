using EcsSync2.Fps;
using MessagePack;

namespace EcsSync2
{
	[Union( 0, typeof( EntityCreatedEvent ) )]
	[Union( 1, typeof( EntityRemovedEvent ) )]
	[Union( 2, typeof( InputChangedEvent ) )]
	[Union( 3, typeof( PlayerConnectedEvent ) )]
	[Union( 4, typeof( TransformMovedEvent ) )]
	[Union( 5, typeof( TransformVelocityChangedEvent ) )]
	public interface IEvent : IReferencable
	{
	}

	[MessagePackObject]
	public abstract class Event : Referencable, IEvent
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
