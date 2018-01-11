using System;
using System.Collections.Generic;
using System.Linq;

namespace EcsSync2
{
	public class CommandQueue : SimulatorComponent
	{
		SortedList<ulong, Queue> m_queues = new SortedList<ulong, Queue>();

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
			return EnsureQueue( userId ).Find( time );
		}

		public CommandFrame FindFirst(ulong userId)
		{
			if( !m_queues.TryGetValue( userId, out Queue queue ) )
				return null;

			if( queue.Count == 0 )
				return null;

			return queue.First;
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

		int RemoveBefore(Queue queue, uint time)
		{
			var count = 0;

			while( queue.Count > 0 && queue.First.Time < time )
			{
				queue.Dequeue().Release();
				count++;
			}

			return count;
		}

		Queue EnsureQueue(ulong userId)
		{
			if( !m_queues.TryGetValue( userId, out Queue queue ) )
			{
				queue = new Queue( userId );
				m_queues.Add( userId, queue );
			}
			return queue;
		}

		public IEnumerable<ulong> UserIds => m_queues.Keys;

		public IEnumerable<Queue> Queues => m_queues.Values;

		public class Queue
		{
			ulong m_userId;
			Queue<CommandFrame> m_frames = new Queue<CommandFrame>();
			uint m_firstFrameTime;
			uint m_lastFrameTime;

			public Queue(ulong userId)
			{
				m_userId = userId;
			}

			public CommandFrame Find(uint time)
			{
				foreach( var f in m_frames )
				{
					if( f.Time == time )
						return f;

					if( f.Time > time )
						break;
				}

				return null;
			}

			public void Enqueue(CommandFrame frame)
			{
				m_frames.Enqueue( frame );

				if( m_firstFrameTime == 0 )
					m_firstFrameTime = frame.Time;

				if( m_lastFrameTime < frame.Time )
					m_lastFrameTime = frame.Time;
			}

			public CommandFrame Dequeue()
			{
				return m_frames.Dequeue();
			}

			public ulong UserId => m_userId;

			public int Count => m_frames.Count;

			public CommandFrame First => m_frames.Peek();

			public CommandFrame Last => m_frames.Last();

			public uint FirstFrameTime => m_firstFrameTime;

			public uint LastFrameTime => m_lastFrameTime;
		}
	}
}
