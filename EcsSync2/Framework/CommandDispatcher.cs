using System.Collections.Generic;

namespace EcsSync2
{
	public class CommandDispatcher : SimulatorComponent
	{
		SortedList<ulong, CommandFrameBuffer> m_buffers = new SortedList<ulong, CommandFrameBuffer>();

		public void Enqueue(ulong userId, CommandFrame frame)
		{
			var buffer = EnsureBuffer( userId );
			buffer.Enqueue( frame );
		}

		CommandFrameBuffer EnsureBuffer(ulong userId)
		{
			if( !m_buffers.TryGetValue( userId, out CommandFrameBuffer buffer ) )
			{
				buffer = new CommandFrameBuffer();
				m_buffers.Add( userId, buffer );
			}

			return buffer;
		}

		internal CommandFrame FetchCommands(ulong userId, uint time)
		{
			var buffer = EnsureBuffer( userId );

			return buffer.Dequeue( time );
		}

		internal override void OnFixedUpdate()
		{
			base.OnFixedUpdate();

			foreach( var buffer in m_buffers.Values )
			{
				// 获取当前帧（和之前未执行）的命令
				CommandFrame frame;
				do
				{
					frame = buffer.Dequeue( Simulator.FixedTime );
					if( frame != null )
						Simulator.ComponentScheduler.EnqueueCommands( frame );
				} while( frame != null );

				// 如果没有取到当前帧的命令，将前移帧的输入作为预测
				if( frame == null || frame.Time != Simulator.FixedTime )
				{
					if( buffer.LastFrame != null )
						Simulator.ComponentScheduler.EnqueueCommands( buffer.LastFrame );

					if( !buffer.IsExhausted )
					{
						//player.connection.ApplyEvent();
						buffer.IsExhausted = true;
					}
				}
				else
				{
					if( buffer.IsExhausted && buffer.FrameCount >= Settings.StopExhaustCommandBufferSize )
					{
						//player.connection.ApplyEvent();
						buffer.IsExhausted = false;
					}
				}
			}
		}

		class CommandFrameBuffer
		{
			Queue<CommandFrame> m_frames = new Queue<CommandFrame>( 3 );
			CommandFrame m_lastFrame;

			public void Enqueue(CommandFrame frame)
			{
				frame.Retain();

				m_frames.Enqueue( frame );
			}

			public CommandFrame Dequeue(uint time)
			{
				if( m_frames.Count > 0 && m_frames.Peek().Time <= time )
				{
					var f = m_frames.Dequeue();

					m_lastFrame.Release();
					m_lastFrame = f;
					m_lastFrame.Retain();

					return f;
				}
				else
					return null;
			}

			public int FrameCount => m_frames.Count;

			public CommandFrame LastFrame => m_lastFrame;

			public bool IsExhausted { get; set; }
		}
	}
}
