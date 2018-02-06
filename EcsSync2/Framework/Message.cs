using ProtoBuf;

namespace EcsSync2
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
	public class LoginRequest : Message
	{
		[ProtoMember( 1 )]
		public ulong UserId;

		[ProtoMember( 2 )]
		public uint ClientTime;
	}

	public enum LoginResult
	{
		Ok,
	}

	[ProtoContract]
	public class LoginResponse : Message
	{
		[ProtoMember( 1 )]
		public LoginResult Result;

		[ProtoMember( 2 )]
		public uint ClientTime;

		[ProtoMember( 3 )]
		public uint ServerTime;
	}

	[ProtoContract]
	public class HeartbeatRequest : Message
	{
		[ProtoMember( 1 )]
		public uint ClientTime;
	}

	[ProtoContract]
	public class HeartbeatResponse : Message
	{
		[ProtoMember( 1 )]
		public uint ClientTime;

		[ProtoMember( 2 )]
		public uint ServerTime;
	}

	[ProtoContract]
	public class MessageEnvelop
	{
		[ProtoMember( 1 )]
		public Message Message;
	}
}
