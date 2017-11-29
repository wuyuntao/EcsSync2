using System.Collections.Generic;
using System.Diagnostics;

namespace EcsSync2
{
	public class ServerTickScheduler : TickScheduler
	{
		TickContext m_context;
		SortedList<ulong, CommandFrame> m_dispatchedCommands = new SortedList<ulong, CommandFrame>();
		uint? m_lastDeltaSyncTime;

		public ServerTickScheduler(Simulator simulator)
			: base( simulator )
		{
		}

		internal override void Tick()
		{
			m_context = new TickContext( TickContextType.Sync, Simulator.FixedTime );

			EnterContext( m_context );
			DispatchCommands( m_context );
			FixedUpdate();
			LeaveContext();

			//Simulator.Context.Log( "Tick {0}", m_context.Time );
		}

		void DispatchCommands(TickContext context)
		{
			foreach( var userId in Simulator.CommandQueue.UserIds )
			{
				if( userId == 0 )
					DispathSimulatorCommands( context );
				else
					DispatchUserCommands( context, userId );
			}
		}

		void DispathSimulatorCommands(TickContext context)
		{
			var frame = Simulator.CommandQueue.Find( 0, context.Time );
			if( frame != null )
				DispatchCommands( frame );
		}

		void DispatchUserCommands(TickContext context, ulong userId)
		{
			// TODO 减少按 UserId 的查询
			m_dispatchedCommands.TryGetValue( userId, out CommandFrame lastFrame );

			// 尝试执行从上一次应用的命令帧开始，到当前帧之间的所有命令
			var lastFrameChanged = false;
			for( var time = GetUserCommandStartTime( context, userId, lastFrame );
				time <= context.Time;
				time += Configuration.SimulationDeltaTime )
			{
				var frame = Simulator.CommandQueue.Find( userId, time );
				if( frame == null )
					break;

				DispatchCommands( frame );
				lastFrame = frame;
				lastFrameChanged = true;

				if( lastFrame.Time != context.Time )
					Simulator.Context.LogWarning( "Re-dispatch commands {0}", frame );
			}

			if( lastFrame != null )
			{
				// 更新当前应用的命令帧
				if( lastFrameChanged )
					m_dispatchedCommands[userId] = lastFrame;

				// 如果无法获取当前帧的话，总是重复上一次的命令帧
				if( lastFrame.Time != context.Time )
				{
					DispatchCommands( lastFrame );
					Simulator.Context.LogWarning( "Dispatch last commands {0} since current frame is not received", lastFrame );

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

		uint GetUserCommandStartTime(TickContext context, ulong userId, CommandFrame lastDispatchedFrame)
		{
			if( lastDispatchedFrame != null )
				return lastDispatchedFrame.Time + Configuration.SimulationDeltaTime;

			var firstUndispatchedFrame = Simulator.CommandQueue.FindFirst( userId );
			if( firstUndispatchedFrame != null )
				return firstUndispatchedFrame.Time;

			return context.Time;
		}

		public DeltaSyncFrame FetchDeltaSyncFrame()
		{
			Debug.Assert( m_lastDeltaSyncTime != null );

			if( m_lastDeltaSyncTime >= m_context.Time )
				return null;

			var f = Simulator.EventBus.FetchEvents( m_lastDeltaSyncTime.Value + Configuration.SimulationDeltaTime );
			m_lastDeltaSyncTime += Configuration.SimulationDeltaTime;
			return f;
		}

		public FullSyncFrame FetchFullSyncFrame()
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

			if( m_lastDeltaSyncTime == null )
				m_lastDeltaSyncTime = m_context.Time;

			return f;
		}

		internal FullSyncFrame FetchFullSyncFrame2()
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

		internal DeltaSyncFrame FetchDeltaSyncFrame2()
		{
			var f = Simulator.EventBus.FetchEvents( m_context.Time );
			f.Retain();
			return f;
		}
	}
}
