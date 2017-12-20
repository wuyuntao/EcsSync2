using ProtoBuf;

namespace EcsSync2.Fps
{
	[ProtoContract]
	[ProtoInclude( 1, typeof( LoginRequest ) )]
	[ProtoInclude( 2, typeof( LoginResponse ) )]
	[ProtoInclude( 3, typeof( HeartbeatRequest ) )]
	[ProtoInclude( 4, typeof( HeartbeatResponse ) )]
	[ProtoInclude( 5, typeof( CommandFrame ) )]
	[ProtoInclude( 6, typeof( SyncFrame ) )]
	public abstract class Message : Referencable
	{
	}

	[ProtoContract]
	public class MessageEnvelop
	{
		[ProtoMember( 1 )]
		public Message Message;
	}
}
