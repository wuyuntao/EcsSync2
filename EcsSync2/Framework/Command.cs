using EcsSync2.Fps;
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
	[ProtoInclude( 1, typeof( PlayerConnectCommand ) )]
	[ProtoInclude( 2, typeof( MoveCharacterCommand ) )]
	public abstract class ComponentCommand : Command
	{
		[ProtoMember( 11 )]
		public uint ComponentId;
	}
}
