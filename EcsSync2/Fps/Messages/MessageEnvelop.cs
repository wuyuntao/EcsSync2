﻿using MessagePack;

namespace EcsSync2.Fps
{
	[Union( 0, typeof( LoginRequestMessage ) )]
	[Union( 1, typeof( LoginResponseMessage ) )]
	[Union( 2, typeof( HeartbeatRequestMessage ) )]
	[Union( 3, typeof( HeartbeatResponseMessage ) )]
	[Union( 4, typeof( CommandFrame ) )]
	[Union( 5, typeof( FullSyncFrame ) )]
	[Union( 6, typeof( DeltaSyncFrame ) )]
	public interface IMessage
	{
	}

	[MessagePackObject]
	public class MessageEnvelop
	{
		[Key( 0 )]
		public IMessage Message;
	}
}
