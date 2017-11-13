using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public class ServerTickScheduler : TickScheduler
	{
		TickContext m_context = new TickContext( TickContextType.Sync );
		SortedList<ulong, CommandFrame> m_dispatchedCommands = new SortedList<ulong, CommandFrame>();

		public ServerTickScheduler(Simulator simulator)
			: base( simulator )
		{
		}

		internal override void Tick()
		{
			m_context.Time = Simulator.FixedTime;

			EnterContext( m_context );
			DispatchCommands( m_context );
			FixedUpdate();
			LeaveContext();
		}

		void DispatchCommands(TickContext context)
		{
			// TODO 合并 CommandQueue
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
			var frame = Simulator.CommandQueue.FetchCommands( 0, context.Time );
			if( frame != null )
				DispatchCommands( frame );
		}

		void DispatchUserCommands(TickContext context, ulong userId)
		{
			// TODO 减少按 UserId 的查询
			m_dispatchedCommands.TryGetValue( userId, out CommandFrame lastFrame );

			// 尝试执行从上一次应用的命令帧开始，到当前帧之间的所有命令
			var lastFrameChanged = false;
			for( var time = lastFrame != null ? lastFrame.Time + context.DeltaTime : context.Time;
				time <= context.Time;
				time += context.DeltaTime )
			{
				var frame = Simulator.CommandQueue.FetchCommands( userId, time );
				if( frame == null )
					break;

				DispatchCommands( frame );
				lastFrame = frame;
				lastFrameChanged = true;
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

					// TODO 按需加速客户端
				}
				else
				{
					// TODO 按需恢复客户端加速
				}

				// 清理已应用命令
				Simulator.CommandQueue.DequeueBefore( userId, lastFrame.Time );
			}
		}

		public DeltaSyncFrame FetchDeltaSyncFrame()
		{
			var f = Simulator.ReferencableAllocator.Allocate<DeltaSyncFrame>();
			f.Time = m_context.Time;

			foreach( var e in Simulator.EventBus.FetchUnsyncedEvents() )
			{
				f.Events.Add( e );
				e.Retain();
			}

			f.Retain();
			return f;
		}

		public FullSyncFrame FetchFullSyncFrame()
		{
			var f = Simulator.ReferencableAllocator.Allocate<FullSyncFrame>();
			f.Time = m_context.Time;

			foreach( var e in Simulator.SceneManager.Entities )
			{
				var s = e.CreateSnapshot();
				f.Entities.Add( s );

				s.Retain();
			}

			f.Retain();
			return f;
		}
	}
}
