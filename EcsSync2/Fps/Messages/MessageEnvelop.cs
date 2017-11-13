using MessagePack;

namespace EcsSync2.Fps
{
	[Union( 0, typeof( LoginRequestMessage ) )]
	[Union( 1, typeof( LoginResponseMessage ) )]
	[Union( 2, typeof( HeartbeatRequestMessage ) )]
	[Union( 3, typeof( HeartbeatResponseMessage ) )]
	[Union( 4, typeof( CommandFrameMessage ) )]
	[Union( 5, typeof( FullSyncFrameMessage ) )]
	[Union( 6, typeof( DeltaSyncFrameMessage ) )]
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
