using EcsSync2.Fps;
using System.Diagnostics;
using System.Threading.Tasks;

namespace EcsSync2.Examples
{
	static class ServerTest
	{
		public static async void Run()
		{
			var simulatorContext = new SimulatorContext( Network.Default );
			var simulator = new Simulator( simulatorContext, true, false, 1, null );
			simulator.SceneManager.LoadScene<BattleScene>();
			simulator.NetworkServer.Start( 3687 );

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

		class Network : INetwork
		{
			public static readonly Network Default = new Network();

			NetworkClient.INetworkClient NetworkClient.IContext.CreateClient()
			{
				return new LiteNetClient( null );
			}

			NetworkServer.INetworkServer NetworkServer.IContext.CreateServer()
			{
				return new LiteNetServer( null );
			}
		}
	}
}
