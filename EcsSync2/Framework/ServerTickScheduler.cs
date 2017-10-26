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

					DispatchCommands( context, frame );
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
						DispatchCommands( context, lastFrame );

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
		}

		void DispatchCommands(TickContext context, CommandFrame frame)
		{
			if( frame.Commands != null )
			{
				foreach( var command in frame.Commands )
				{
					var component = Simulator.SceneManager.FindComponent( command.Receiver );
					component.ReceiveCommand( command );
				}
			}
		}
	}
}
