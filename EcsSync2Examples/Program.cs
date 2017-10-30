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
			simulator.SceneManager.LoadScene<BattleScene>();

			var frame = simulator.ReferencableAllocator.Allocate<CommandFrame>();
			frame.Time = Configuration.SimulationDeltaTime;
			var command = frame.AddCommand<CreateEntityCommand>();
			command.Settings = new GameManagerSettings();
			simulator.CommandQueue.EnqueueCommands( 0, frame );
			frame.Release();

			for( int i = 0; i < 1000; i++ )
			{
				simulator.Simulate( Configuration.SimulationDeltaTime );
				Console.WriteLine( $"Simulate #{i}" );
			}
		}
	}
}
