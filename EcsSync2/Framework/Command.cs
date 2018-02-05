using ProtoBuf;

namespace EcsSync2
{
	[ProtoContract]
	[ProtoInclude( 1, typeof( SceneCommand ) )]
	[ProtoInclude( 2, typeof( ComponentCommand ) )]
	public class Command : SerializableReferencable
	{
	}

	[ProtoContract]
	[ProtoInclude( 1, typeof( CreateEntityCommand ) )]
	[ProtoInclude( 2, typeof( RemoveEntityCommand ) )]
	public abstract class SceneCommand : Command
	{
	}

	[ProtoContract]
	public abstract class ComponentCommand : Command
	{
		[ProtoMember( 11 )]
		public uint ComponentId;
	}
}
