using EcsSync2.Fps;
using ProtoBuf;

namespace EcsSync2
{
	[ProtoContract]
	[ProtoInclude( 1, typeof( SceneEvent ) )]
	[ProtoInclude( 2, typeof( ComponentEvent ) )]
	public abstract class Event : SerializableReferencable
	{
	}

	[ProtoContract]
	[ProtoInclude( 1, typeof( EntityCreatedEvent ) )]
	[ProtoInclude( 2, typeof( EntityRemovedEvent ) )]
	public abstract class SceneEvent : Event
	{
	}

	[ProtoContract]
	[ProtoInclude( 1, typeof( InputChangedEvent ) )]
	[ProtoInclude( 2, typeof( PlayerConnectedEvent ) )]
	[ProtoInclude( 3, typeof( TransformMovedEvent ) )]
	[ProtoInclude( 4, typeof( TransformVelocityChangedEvent ) )]
	public abstract class ComponentEvent : Event
	{
		[ProtoMember( 11 )]
		public uint ComponentId;

		protected override void OnReset()
		{
			ComponentId = 0;

			base.OnReset();
		}
	}
}
