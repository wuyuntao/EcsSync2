using MessagePack;

namespace EcsSync2.Fps
{
	[MessagePackObject]
	public class LoginRequestMessage : IMessage
	{
		[Key( 0 )]
		public ulong UserId;

		[Key( 1 )]
		public uint ClientTime;
	}

	[MessagePackObject]
	public class LoginResponseMessage : IMessage
	{
		[Key( 0 )]
		public bool Ok;

		[Key( 1 )]
		public uint ClientTime;

		[Key( 2 )]
		public uint ServerTime;
	}

	[MessagePackObject]
	public class HeartbeatRequestMessage : IMessage
	{
		[Key( 0 )]
		public uint ClientTime;
	}

	[MessagePackObject]
	public class HeartbeatResponseMessage : IMessage
	{
		[Key( 0 )]
		public uint ClientTime;

		[Key( 1 )]
		public uint ServerTime;
	}
}
