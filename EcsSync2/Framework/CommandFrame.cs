using ProtoBuf;
using System.Collections.Generic;

namespace EcsSync2
{
	[ProtoContract]
	public class CommandFrame : Message
	{
		[ProtoMember( 1 )]
		public ulong UserId;

		[ProtoMember( 2 )]
		public uint Time;

		[ProtoMember( 3 )]
		public List<Command> Commands = new List<Command>();

		public override string ToString()
		{
			return $"{GetType().Name}<User: {UserId}, Time: {Time}, Commands: {Commands.Count}>";
		}

		public T AddCommand<T>()
			where T : Command, new()
		{
			var command = ReferenceCounter.Allocate<T>();
			Commands.Add( command );
			return command;
		}

		protected override void OnAllocate()
		{
			foreach( var c in Commands )
				this.Allocate( c );

			base.OnAllocate();
		}

		protected override void OnReset()
		{
			foreach( var c in Commands )
				c.Release();

			Commands.Clear();

			base.OnReset();
		}
	}
}
