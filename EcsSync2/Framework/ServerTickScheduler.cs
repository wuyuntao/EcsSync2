using System.Collections.Generic;

namespace EcsSync2
{
	public class ServerTickScheduler : TickScheduler
	{
		TickContext m_context = new TickContext( TickContextType.Sync, 0 );
		SortedList<ulong, CommandFrame> m_dispatchedCommands = new SortedList<ulong, CommandFrame>();
		SortedList<uint, DeltaSyncFrame> m_deltaSyncFrames = new SortedList<uint, DeltaSyncFrame>();

		public ServerTickScheduler(Simulator simulator)
			: base( simulator )
		{
		}

		internal override void Tick()
		{
			Simulator.NetworkServer.ReceiveMessages();

			for( int i = 0; i < Configuration.MaxTickCount; i++ )
			{
				var nextTime = ( m_context.Time + Configuration.SimulationDeltaTime ) / 1000f;
				if( Simulator.SynchronizedClock.Time < nextTime )
					break;

				m_context = new TickContext( TickContextType.Sync, m_context.Time + Configuration.SimulationDeltaTime );

				EnterContext( m_context );
				DispatchCommands();
				FixedUpdate();
				Simulator.EventDispatcher.Dispatch();
				LeaveContext();

				Simulator.NetworkServer.SendMessages();

				//Simulator.Context.Log( "Tick {0}", m_context.Time );
			}
		}

		void DispatchCommands()
		{
			foreach( var userId in Simulator.CommandQueue.UserIds )
			{
				if( userId == 0 )
					DispathSimulatorCommands();
				else
					DispatchUserCommands( userId );
			}
		}

		void DispathSimulatorCommands()
		{
			var frame = Simulator.CommandQueue.Find( 0, m_context.Time );
			if( frame != null )
				DispatchCommands( frame );
		}

		void DispatchUserCommands(ulong userId)
		{
			// TODO 减少按 UserId 的查询
			m_dispatchedCommands.TryGetValue( userId, out CommandFrame lastFrame );
			var lastFrameChanged = false;

			// 尝试执行从上一次应用的命令帧开始，到当前帧之间的所有命令
			int dispatchedCommands = 0;
			for( var time = GetUserCommandStartTime( userId, lastFrame );
				time <= m_context.Time && dispatchedCommands < Configuration.MaxCommandDispatchCount;
				time += Configuration.SimulationDeltaTime )
			{
				var frame = Simulator.CommandQueue.Find( userId, time );
				if( frame == null )
					break;

				DispatchCommands( frame );
				lastFrame = frame;
				lastFrameChanged = true;

				//if( lastFrame.Time != context.Time )
				//	Simulator.Context.LogWarning( "Re-dispatch commands {0}", frame );

				++dispatchedCommands;
			}

			if( lastFrame != null )
			{
				// 更新当前应用的命令帧
				if( lastFrameChanged )
					m_dispatchedCommands[userId] = lastFrame;

				// 如果无法获取当前帧的话，总是重复上一次的命令帧
				if( lastFrame.Time != m_context.Time )
				{
					DispatchCommands( lastFrame );
					//Simulator.Context.LogWarning( "Dispatch last commands {0} since current frame is not received", lastFrame );

					// TODO 按需加速客户端
				}
				else
				{
					// TODO 按需恢复客户端加速
				}

				// 清理已应用命令
				Simulator.CommandQueue.RemoveBefore( userId, lastFrame.Time );
			}
		}

		uint GetUserCommandStartTime(ulong userId, CommandFrame lastDispatchedFrame)
		{
			if( lastDispatchedFrame != null )
				return lastDispatchedFrame.Time + Configuration.SimulationDeltaTime;

			var firstUndispatchedFrame = Simulator.CommandQueue.FindFirst( userId );
			if( firstUndispatchedFrame != null )
				return firstUndispatchedFrame.Time;

			return m_context.Time;
		}

		internal void EnqueueEvent(Event @event)
		{
			//Simulator.Context.Log( "AddEvent {0}ms {1}", time, @event );

			var frame = EnsureFrame( m_context.Time );
			@event.Retain();
			frame.Events.Add( @event );
		}

		DeltaSyncFrame EnsureFrame(uint time)
		{
			if( !m_deltaSyncFrames.TryGetValue( time, out DeltaSyncFrame frame ) )
			{
				frame = Simulator.ReferencableAllocator.Allocate<DeltaSyncFrame>();
				frame.Time = time;

				frame.Retain();
				m_deltaSyncFrames.Add( time, frame );
			}
			return frame;
		}

		internal FullSyncFrame FetchFullSyncFrame()
		{
			var f = Simulator.ReferencableAllocator.Allocate<FullSyncFrame>();
			f.Time = m_context.Time;

			EnterContext( m_context );
			foreach( var e in Simulator.SceneManager.Entities )
			{
				var s = e.CreateSnapshot();
				f.Entities.Add( s );

				s.Retain();
			}
			LeaveContext();
			return f;
		}

		internal DeltaSyncFrame FetchDeltaSyncFrame()
		{
			var f = EnsureFrame( m_context.Time );
			f.Retain();
			return f;
		}
	}
}
