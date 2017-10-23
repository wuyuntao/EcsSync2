using System.Collections.Generic;

namespace EcsSync2
{
	public class CommandFrame : Message
	{
		public uint Time;

		public List<Command> Commands;

		public void AddCommand(Command command)
		{
			Commands = Commands ?? new List<Command>( 1 );
			Commands.Add( command );
		}
	}
}
