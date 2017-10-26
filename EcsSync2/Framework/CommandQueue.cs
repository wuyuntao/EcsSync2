using System.Collections.Generic;

namespace EcsSync2
{
	public class CommandQueue : SimulatorComponent
	{
		SortedList<ulong, Queue<CommandFrame>> m_queues = new SortedList<ulong, Queue<CommandFrame>>();

		public CommandQueue(Simulator simulator)
			: base( simulator )
		{
		}

		public void EnqueueCommands(ulong userId, CommandFrame frame)
		{
			frame.Retain();

			EnsureQueue( userId ).Enqueue( frame );
		}

		public CommandFrame FetchCommands(ulong userId, uint time)
		{
			foreach( var f in EnsureQueue( userId ) )
			{
				if( f.Time == time )
					return f;

				if( f.Time > time )
					break;
			}

			return null;
		}

		public void DequeueBefore(ulong userId, uint time)
		{
			var queue = EnsureQueue( userId );

			while( queue.Count > 0 && queue.Peek().Time < time )
				queue.Dequeue().Release();
		}

		Queue<CommandFrame> EnsureQueue(ulong userId)
		{
			if( !m_queues.TryGetValue( userId, out Queue<CommandFrame> queue ) )
			{
				queue = new Queue<CommandFrame>();
				m_queues.Add( userId, queue );
			}
			return queue;
		}

		public IEnumerable<ulong> UserIds => m_queues.Keys;
	}
}
