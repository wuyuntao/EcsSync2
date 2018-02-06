using EcsSync2.Fps;
using System;
using System.Diagnostics;

namespace EcsSync2.Examples
{
	static class StandaloneTest
	{
		public static void Run()
		{
			var simulatorContext = new SimulatorContext( null );
			var simulator = new Simulator( simulatorContext, true, true, 1, 1 );
			simulator.SceneManager.LoadScene<BattleScene>();

			bool isPlayerCreated = false;
			bool isPlayerConnected = false;

			var r = new Random();
			var stopwatch = Stopwatch.StartNew();
			var deltaTime = 16;
			var lastSimulateTime = 0f;
			for( int i = 1; ; i++ )
			{
				//await Task.Delay( deltaTime );

				var elaspedSecs = i * deltaTime / 1000f;
				if( elaspedSecs > 1 && !isPlayerCreated )
				{
					var f = simulator.ReferencableAllocator.Allocate<CommandFrame>();
					f.Time = simulator.StandaloneTickScheduler.Time + Configuration.SimulationDeltaTime;
					f.Retain();
					var c = f.AddCommand<CreateEntityCommand>();
					c.Settings = new PlayerSettings() { UserId = simulator.LocalUserId.Value };

					simulator.CommandQueue.Add( 0, f );
					isPlayerCreated = true;
				}
				else if( elaspedSecs > 2 && isPlayerCreated && !isPlayerConnected )
				{
					simulatorContext.SetButton( "Jump", true );

					isPlayerConnected = true;
				}
				else if( elaspedSecs > 3 && isPlayerCreated && isPlayerConnected )
				{
					simulatorContext.SetAxis( "Horizontal", (float)r.NextDouble() );
					simulatorContext.SetAxis( "Vertical", (float)r.NextDouble() );
				}

				simulator.Simulate( elaspedSecs - lastSimulateTime );
				lastSimulateTime = elaspedSecs;
			}
		}
	}
}
