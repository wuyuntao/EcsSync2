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
	public abstract class ComponentEvent : Event
	{
		[ProtoMember( 11 )]
		public uint ComponentId;
	}
}
