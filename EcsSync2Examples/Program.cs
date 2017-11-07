using EcsSync2.Fps;
using MessagePack;
using System;

namespace EcsSync2.Examples
{
	class Program
	{
		static void Main(string[] args)
		{
			//TestSerializer();
			//RunStandaloneSimulator();

			RunServer();
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

		static void RunServer()
		{
			var server = new FpsServer( new SimulatorContext(), "0.0.0.0", 3687 );

			Console.WriteLine( "started" );
			Console.ReadLine();

			server.Stop();
		}
	}
}
