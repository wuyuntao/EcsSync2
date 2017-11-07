using MessagePack;

namespace EcsSync2.Fps
{
	[MessagePackObject]
	public class DeltaSyncFrameMessage : IFrameMessage
	{
		[Key( 0 )]
		public uint Time;
	}

	[MessagePackObject]
	public class EntityCreatedEventMessage : IEventUnion
	{
		[Key( 0 )]
		public uint Id;

		[Key( 1 )]
		public IEntitySettingsUnion Settings;
	}

	[MessagePackObject]
	public class EntityRemovedEventMessage : IEventUnion
	{
		[Key( 0 )]
		public uint Id;
	}

	[Union( 0, typeof( EntityCreatedEventMessage ) )]
	[Union( 1, typeof( EntityRemovedEventMessage ) )]
	[Union( 2, typeof( InputChangedEvent ) )]
	[Union( 3, typeof( PlayerConnectedEvent ) )]
	[Union( 4, typeof( TransformMovedEvent ) )]
	[Union( 5, typeof( TransformVelocityChangedEvent ) )]
	public interface IEventUnion
	{
	}
}
