using EcsSync2.Fps;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EcsSync2.Examples
{
	static class ServerTest
	{

		public static async void Run()
		{
			var simulatorContext = new SimulatorContext( createServer: CreateServer );
			var simulator = SimulatorBootstrap.StartServer( simulatorContext, 1, 3687 );

			var stopwatch = Stopwatch.StartNew();
			var deltaTime = 8;
			var lastSimulateTime = 0f;
			for( int i = 1; ; i++ )
			{
				await Task.Delay( deltaTime );

				var elaspedSecs = (float)stopwatch.Elapsed.TotalSeconds;
				simulator.Simulate( elaspedSecs - lastSimulateTime );
				lastSimulateTime = elaspedSecs;
			}
		}

		public static NetworkServer.INetworkServer CreateServer() => new LiteNetServer( null );
	}
}
