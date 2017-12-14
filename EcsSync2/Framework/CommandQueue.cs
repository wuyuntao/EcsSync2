﻿using System.Collections.Generic;

namespace EcsSync2
{
	public class CommandQueue : SimulatorComponent
	{
		SortedList<ulong, Queue<CommandFrame>> m_queues = new SortedList<ulong, Queue<CommandFrame>>();

		public CommandQueue(Simulator simulator)
			: base( simulator )
		{
		}

		public void Add(ulong userId, CommandFrame frame)
		{
			//if( frame.Commands.Count > 0 && Simulator.IsServer )
			//	Simulator.Context.Log( "EqueueCommands for {0}, {1}, {2}", userId, frame, frame.Time );
			if( Simulator.ServerTickScheduler != null )
			{
				if( frame.Time <= Simulator.ServerTickScheduler.Time )
					Simulator.Context.LogWarning( "Received late command from user {0}, Time: {1} <= {2}", userId, frame.Time, Simulator.ServerTickScheduler.Time );
			}

			frame.Retain();

			EnsureQueue( userId ).Enqueue( frame );
		}

		public CommandFrame Find(ulong userId, uint time)
		{
			return Find( EnsureQueue( userId ), time );
		}

		public CommandFrame FindFirst(ulong userId)
		{
			if( !m_queues.TryGetValue( userId, out Queue<CommandFrame> queue ) )
				return null;

			if( queue.Count == 0 )
				return null;

			return queue.Peek();
		}

		CommandFrame Find(Queue<CommandFrame> queue, uint time)
		{
			foreach( var f in queue )
			{
				if( f.Time == time )
					return f;

				if( f.Time > time )
					break;
			}

			return null;
		}

		public int RemoveBefore(ulong userId, uint time)
		{
			return RemoveBefore( EnsureQueue( userId ), time );
		}

		public int RemoveBefore(uint time)
		{
			var count = 0;
			for( int i = 0; i < m_queues.Values.Count; ++i )
			{
				var q = m_queues.Values[i];
				count += RemoveBefore( q, time );
			}
			return count;
		}

		int RemoveBefore(Queue<CommandFrame> queue, uint time)
		{
			var count = 0;

			while( queue.Count > 0 && queue.Peek().Time < time )
			{
				queue.Dequeue().Release();
				count++;
			}

			return count;
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
