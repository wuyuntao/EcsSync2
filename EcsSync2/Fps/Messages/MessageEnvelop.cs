using MessagePack;

namespace EcsSync2.Fps
{
	[Union( 0, typeof( CommandFrameMessage ) )]
	[Union( 1, typeof( FullSyncFrameMessage ) )]
	[Union( 2, typeof( DeltaSyncFrameMessage ) )]
	public interface IFrameMessage
	{
	}

	[MessagePackObject]
	public class MessageEnvelop
	{
		[Key( 0 )]
		public IFrameMessage Frame;
	}
}
