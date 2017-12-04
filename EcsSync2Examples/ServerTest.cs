using EcsSync2.Fps;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EcsSync2.Examples
{
	static class ServerTest
	{
		public static async void Run()
		{
			var server = new LiteNetServer( null );
			var simulatorContext = new SimulatorContext( server );
			var simulator = new Simulator( simulatorContext, true, false, 1, null );
			simulator.SceneManager.LoadScene<BattleScene>();
			simulator.NetworkServer.Start( 3687 );

			var stopwatch = Stopwatch.StartNew();
			var lastSimulateTime = 0f;
			for( int i = 1; ; i++ )
			{
				var elaspedMs = (int)stopwatch.ElapsedMilliseconds;
				var targetMs = (int)( i * Configuration.SimulationDeltaTime );
				if( targetMs > elaspedMs )
					await Task.Delay( targetMs - elaspedMs );

				var elaspedSecs = (float)stopwatch.Elapsed.TotalSeconds;
				simulator.Simulate( elaspedSecs - lastSimulateTime );
				lastSimulateTime = elaspedSecs;
			}
		}
	}
}
