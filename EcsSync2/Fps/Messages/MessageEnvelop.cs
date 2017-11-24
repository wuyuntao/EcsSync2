using ProtoBuf;

namespace EcsSync2.Fps
{
	[ProtoContract]
	[ProtoInclude( 1, typeof( LoginRequestMessage ) )]
	[ProtoInclude( 2, typeof( LoginResponseMessage ) )]
	[ProtoInclude( 3, typeof( HeartbeatRequestMessage ) )]
	[ProtoInclude( 4, typeof( HeartbeatResponseMessage ) )]
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
