using System;
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

		internal override void OnFixedUpdate()
		{
			base.OnFixedUpdate();

			foreach( var player in GetPlayers() )
			{
				var buffer = EnsureBuffer( player.UserId );

				CommandFrame frame;
				do
				{
					frame = buffer.Dequeue( Simulator.FixedTime );
					Dispatch( player, frame );
				} while( frame != null );

				// 如果没有取到当前时间的命令的话
				if( frame == null || frame.Time != Simulator.FixedTime )
				{
					if( buffer.LastFrame != null )
						Dispatch( player, buffer.LastFrame );

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

		IEnumerable<Player> GetPlayers()
		{
			throw new NotImplementedException();
		}

		void Dispatch(Player player, CommandFrame frame)
		{
			if( frame.Commands != null )
			{
				foreach( var c in frame.Commands )
					Dispatch( player, c );
			}
		}

		void Dispatch(Player player, Command command)
		{
			// 检查命令的有效性
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
