using MessagePack;

namespace EcsSync2.Fps
{
	[Union( 0, typeof( LoginRequestMessage ) )]
	[Union( 1, typeof( LoginResponseMessage ) )]
	[Union( 2, typeof( CommandFrameMessage ) )]
	[Union( 3, typeof( FullSyncFrameMessage ) )]
	[Union( 4, typeof( DeltaSyncFrameMessage ) )]
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
