using System.Collections.Generic;

namespace EcsSync2
{
	public class CommandFrame : Message
	{
		public uint Time;

		public List<Command> Commands;

		public T AddCommand<T>()
			where T : Command, new()
		{
			var command = ReferenceCounter.Allocate<T>();
			Commands = Commands ?? new List<Command>();
			Commands.Add( command );
			return command;
		}

		protected override void Reset()
		{
			if( Commands != null )
			{
				foreach( var command in Commands )
					command.Release();
			}

			base.Reset();
		}
	}
}
