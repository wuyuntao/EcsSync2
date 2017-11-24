using ProtoBuf;

namespace EcsSync2.Fps
{
	[ProtoContract]
	public class LoginRequestMessage : Message
	{
		[ProtoMember( 1 )]
		public ulong UserId;

		[ProtoMember( 2 )]
		public uint ClientTime;
	}

	[ProtoContract]
	public class LoginResponseMessage : Message
	{
		[ProtoMember( 1 )]
		public bool Ok;

		[ProtoMember( 2 )]
		public uint ClientTime;

		[ProtoMember( 3 )]
		public uint ServerTime;
	}

	[ProtoContract]
	public class HeartbeatRequestMessage : Message
	{
		[ProtoMember( 1 )]
		public uint ClientTime;
	}

	[ProtoContract]
	public class HeartbeatResponseMessage : Message
	{
		[ProtoMember( 1 )]
		public uint ClientTime;

		[ProtoMember( 2 )]
		public uint ServerTime;
	}
}
