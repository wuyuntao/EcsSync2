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

			for( int i = 0; i < 1000; i++ )
				simulator.Simulate( Settings.SimulationDeltaTime );
		}
	}
}
