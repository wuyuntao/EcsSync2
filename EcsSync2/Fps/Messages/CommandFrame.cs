using MessagePack;
using System.Collections.Generic;

namespace EcsSync2.Fps
{
	[MessagePackObject]
	public class CommandFrameMessage : IMessage
	{
		[Key( 0 )]
		public ulong UserId;

		[Key( 1 )]
		public uint Time;

		[Key( 2 )]
		public List<ICommandUnion> Commands;

		public static CommandFrameMessage FromCommandFrame(ulong userId, CommandFrame frame)
		{
			var m = new CommandFrameMessage()
			{
				UserId = userId,
				Time = frame.Time,
				Commands = new List<ICommandUnion>(),
			};

			foreach( var c in frame.Commands )
				m.Commands.Add( (ICommandUnion)c );

			return m;
		}

		public CommandFrame ToCommandFrame(Simulator simulator)
		{
			var frame = simulator.ReferencableAllocator.Allocate<CommandFrame>();
			frame.Time = Time;
			foreach( var c in Commands )
				frame.Commands.Add( (Command)c );
			return frame;
		}
	}

	[Union( 0, typeof( CreateEntityCommand ) )]
	[Union( 1, typeof( RemoveEntityCommand ) )]
	[Union( 2, typeof( PlayerConnectCommand ) )]
	[Union( 3, typeof( MoveCharacterCommand ) )]
	public interface ICommandUnion
	{
	}
}
