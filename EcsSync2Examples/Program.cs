using EcsSync2.Fps;
using MessagePack;
using System;
using System.Diagnostics;
using System.Threading;

namespace EcsSync2.Examples
{
	class Program
	{
		static bool m_end;

		static void Main(string[] args)
		{
			//TestSerializer();
			//RunStandaloneSimulator();

			RunServer( null );
			//ThreadPool.QueueUserWorkItem( RunServer );
			//ThreadPool.QueueUserWorkItem( RunClient );

			Console.WriteLine( "Press any key to exit" );
			Console.ReadLine();

			m_end = true;
		}


		static void TestSerializer()
		{
			var s0 = new PlayerSettings() { UserId = 100, IsAI = true };
			var bytes = MessagePackSerializer.Serialize( s0 );
			var s1 = MessagePackSerializer.Deserialize<PlayerSettings>( bytes );
		}

		static void RunStandaloneSimulator()
		{
			var context = new SimulatorContext();
			var simulator = new Simulator( context, true, true, 1, 1 );
			var scene = simulator.SceneManager.LoadScene<BattleScene>();

			for( int i = 0; i < 1000; i++ )
			{
				if( i == 0 )
				{
					EnqueueCommand<CreateEntityCommand>( simulator, c => c.Settings = new GameManagerSettings() );
				}
				else if( i == 1 )
				{
					EnqueueCommand<CreateEntityCommand>( simulator, c => c.Settings = new PlayerSettings() { UserId = 1, IsAI = false } );
				}
				else if( i == 2 )
				{
					EnqueueCommand<PlayerConnectCommand>( simulator, c => c.Receiver = 65 );
				}
				else if( i >= 3 && i <= 10 )
				{
					EnqueueCommand<MoveCharacterCommand>( simulator, c =>
					{
						c.Receiver = 98;
						c.InputDirection = new Vector2D( 0, 1 );
						c.InputMagnitude = 1;
					} );
				}

				simulator.Simulate( Configuration.SimulationDeltaTime / 1000f );
				Console.WriteLine( $"Simulate #{i + 1}, {simulator.FixedTime}ms" );
			}
		}

		static void EnqueueCommand<T>(Simulator simulator, Action<T> handler)
			where T : Command, new()
		{
			var frame = simulator.ReferencableAllocator.Allocate<CommandFrame>();
			frame.Time = simulator.FixedTime + Configuration.SimulationDeltaTime;
			var command = frame.AddCommand<T>();
			handler( command );
			simulator.CommandQueue.EnqueueCommands( 0, frame );
			frame.Release();
		}

		static void RunServer(object state)
		{
			var server = new FpsServer( new SimulatorContext(), "0.0.0.0", 3687 );
			var clock = new Clock();

			while( !m_end )
			{
				server.Update();
				clock.Tick();
			}

			server.Stop();
		}

		static void RunClient(object state)
		{
			Console.WriteLine( "wait server started" );
			Thread.Sleep( 3000 );

			var client = new FpsClient( new SimulatorContext(), "192.168.92.144", 3687, 1 );
			var clock = new Clock();

			while( !m_end )
			{
				client.Update();
				clock.Tick();
			}

			client.Stop();
		}

		class Clock
		{
			int m_ticks;
			Stopwatch m_sw = Stopwatch.StartNew();

			public void Tick()
			{
				m_ticks++;

				var ms = m_ticks * Configuration.SynchronizationDeltaTime - (int)m_sw.ElapsedMilliseconds;
				if( ms > 0 )
					Thread.Sleep( ms );
			}
		}
	}
}
