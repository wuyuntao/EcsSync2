using EcsSync2.Fps;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EcsSync2.Examples
{
	class FakeNetworkTest
	{
		static readonly TimeSpan TickInterval = TimeSpan.FromSeconds( 1 / 60f );

		static object s_timeLock = new object();
		static int s_timeVersion;
		static float s_deltaTime;
		static float s_time;

		static long s_userId;
		static Launcher s_launcher;

		public static void Run()
		{
			using( s_launcher = new Launcher() )
			{
				Console.WriteLine( "Press Ctrl + C to exit" );

				Task.Run( (Action)RunServer );
				Task.Run( (Action)RunClient );
				Task.Run( (Action)RunClient );
				Task.Run( (Action)RunClient );
				Task.Run( (Action)RunClient );

				WaitForShutdown();
			}
		}

		static void WaitForShutdown()
		{
			using( var shutdown = new ManualResetEvent( false ) )
			{
				Console.CancelKeyPress += (s, e) =>
				{
					e.Cancel = true;
					shutdown.Set();
				};

				shutdown.WaitOne();
			}
		}

		async static void RunServer()
		{
			var simulatorContext = new SimulatorContext( createServer: s_launcher.Network.CreateServer );
			var simulator = new Simulator( simulatorContext, true, false, 1, null );
			simulator.SceneManager.LoadScene<BattleScene>();
			simulator.NetworkServer.Start( 5000 );

			var lastTimeVersion = -1;
			var lastDeltaTime = 0f;
			var lastTime = 0f;

			while( true )
			{
				await Task.Delay( TickInterval );

				if( FetchDeltaTime( ref lastTimeVersion, ref lastDeltaTime, ref lastTime ) )
					simulator.Simulate( lastDeltaTime );
			}
		}

		async static void RunClient()
		{
			var lastTimeVersion = -1;
			var lastDeltaTime = 0f;
			var lastTime = 0f;

			while( lastTime < 0.1f )
			{
				if( FetchDeltaTime( ref lastTimeVersion, ref lastDeltaTime, ref lastTime ) )
					await Task.Delay( TickInterval );
			}

			var simulatorContext = new SimulatorContext( createClient: s_launcher.Network.CreateClient );
			var userId = Interlocked.Increment( ref s_userId );
			var simulator = new Simulator( simulatorContext, false, true, 1, (ulong)userId );
			simulator.SceneManager.LoadScene<BattleScene>();
			simulator.NetworkClient.Start( "127.0.0.1", 5000 );

			var r = new Random( Environment.TickCount + (int)userId );

			while( true )
			{
				await Task.Delay( TickInterval );

				if( FetchDeltaTime( ref lastTimeVersion, ref lastDeltaTime, ref lastTime ) )
				{
					if( lastTime > 1 && lastTime < 1.5f )
					{
						simulatorContext.SetButton( "Jump", true );
					}
					else if( lastTime > 2 )
					{
						simulatorContext.SetAxis( "Horizontal", (float)r.NextDouble() );
						simulatorContext.SetAxis( "Vertical", (float)r.NextDouble() );
					}

					simulator.Simulate( lastDeltaTime );

					simulatorContext.ResetButtons();
				}
			}
		}

		static bool FetchDeltaTime(ref int lastTimeVersion, ref float lastDeltaTime, ref float lastTime)
		{
			lock( s_timeLock )
			{
				if( lastTimeVersion == s_timeVersion )
					return false;

				lastTimeVersion = s_timeVersion;
				lastDeltaTime = s_deltaTime;
				lastTime = s_time;
				return true;
			}
		}

		class Launcher : Disposable
		{
			public bool IsRunning = true;
			public FakeNetwork Network = new FakeNetwork();

			public Launcher()
			{
				Task.Run( (Action)Tick );
			}

			protected override void DisposeManaged()
			{
				IsRunning = false;

				base.DisposeManaged();
			}

			async void Tick()
			{
				while( IsRunning )
				{
					await Task.Delay( TickInterval );

					lock( s_timeLock )
					{
						s_deltaTime = (float)TickInterval.TotalSeconds;
						s_time += s_deltaTime;
						s_timeVersion++;

						//Logger.Log( "#{0} Tick. deltaTime: {1}s, time: {2}s", s_timeVersion, s_deltaTime, s_time );
					}

					Network.InvokeActions( s_time );
				}
			}
		}
	}
}
