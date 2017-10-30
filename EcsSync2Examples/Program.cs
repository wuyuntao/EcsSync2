using EcsSync2.FpsExample;
using System;

namespace EcsSync2.Examples
{
	class Program
	{
		static void Main(string[] args)
		{
			RunStandaloneSimulator();
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
	}
}
