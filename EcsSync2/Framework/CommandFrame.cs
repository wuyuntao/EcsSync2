using System.Collections.Generic;

namespace EcsSync2
{
	public class CommandFrame : Message
	{
		public uint Time;

		public List<Command> Commands = new List<Command>();

		public T AddCommand<T>()
			where T : Command, new()
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
