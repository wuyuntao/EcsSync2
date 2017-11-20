using EcsSync2.Fps;
using MessagePack;
using System.Collections.Generic;

namespace EcsSync2
{
	[MessagePackObject]
	public class CommandFrame : Referencable, IMessage
	{
		[Key( 0 )]
		public ulong UserId;

		[Key( 1 )]
		public uint Time;

		[Key( 2 )]
		public List<ICommand> Commands = new List<ICommand>();

		public T AddCommand<T>()
			where T : class, ICommand, new()
		{
			var command = ReferenceCounter.Allocate<T>();
			Commands.Add( command );
			return command;
		}

		protected override void Reset()
		{
			foreach( var c in Commands )
				c.Release();

			Commands.Clear();

			base.Reset();
		}
	}
}
