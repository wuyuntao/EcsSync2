using ProtoBuf;

namespace EcsSync2.Fps
{
	[ProtoContract]
	public class LoginRequest : Message
	{
		[ProtoMember( 1 )]
		public ulong UserId;

		[ProtoMember( 2 )]
		public uint ClientTime;
	}

	[ProtoContract]
	public class LoginResponse : Message
	{
		[ProtoMember( 1 )]
		public bool Ok;

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
}
