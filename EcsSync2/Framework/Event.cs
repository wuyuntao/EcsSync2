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
	[ProtoInclude( 5, typeof( JumpStartedEvent ) )]
	[ProtoInclude( 6, typeof( JumpStoppedEvent ) )]
	[ProtoInclude( 7, typeof( AnimatorStateChangedEvent ) )]
	[ProtoInclude( 8, typeof( AnimatorBoolParameterChangedEvent ) )]
	[ProtoInclude( 9, typeof( AnimatorIntParameterChangedEvent ) )]
	[ProtoInclude( 10, typeof( AnimatorFloatParameterChangedEvent ) )]
	public abstract class ComponentEvent : Event
	{
		[ProtoMember( 11 )]
		public uint ComponentId;
	}
}
