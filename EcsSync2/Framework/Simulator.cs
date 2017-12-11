using System;

namespace EcsSync2
{
	public class Simulator
	{
		public interface IContext : ILogger
		{
		}

		public IContext Context { get; }
		public bool IsServer { get; }
		public bool IsClient { get; }
		public Random Random { get; }
		public ulong? LocalUserId { get; }

		public ReferencableAllocator ReferencableAllocator { get; }
		public SynchronizedClock SynchronizedClock { get; }
		public InputManager InputManager { get; }
		public CommandQueue CommandQueue { get; }
		public InstanceIdAllocator InstanceIdAllocator { get; }
		public SceneManager SceneManager { get; }
		public TickScheduler TickScheduler { get; }
		public StandaloneTickScheduler StandaloneTickScheduler { get; }
		public ServerTickScheduler ServerTickScheduler { get; }
		public ClientTickScheduler ClientTickScheduler { get; }
		public EventDispatcher EventDispatcher { get; }
		public RenderManager RenderManager { get; }

		public NetworkManager NetworkManager { get; }
		public NetworkClient NetworkClient { get; }
		public NetworkServer NetworkServer { get; }

		public Simulator(IContext context, bool isServer, bool isClient, int? randomSeed, ulong? localUserId)
		{
			Context = context;
			IsServer = isServer;
			IsClient = isClient;
			Random = randomSeed != null ? new Random( randomSeed.Value ) : null;
			LocalUserId = localUserId;

			ReferencableAllocator = new ReferencableAllocator( this );
			SynchronizedClock = new SynchronizedClock( this );
			CommandQueue = new CommandQueue( this );
			InstanceIdAllocator = new InstanceIdAllocator( this );
			SceneManager = new SceneManager( this );
			EventDispatcher = new EventDispatcher( this );

			if( isClient )
			{
				InputManager = new InputManager( this );
				RenderManager = new RenderManager( this );
			}

			var isStandalone = isServer && isClient;
			if( isStandalone )
			{
				TickScheduler = StandaloneTickScheduler = new StandaloneTickScheduler( this );
			}
			else if( isServer )
			{
				NetworkManager = NetworkServer = new NetworkServer( this );
				TickScheduler = ServerTickScheduler = new ServerTickScheduler( this );
			}
			else
			{
				NetworkManager = NetworkClient = new NetworkClient( this );
				TickScheduler = ClientTickScheduler = new ClientTickScheduler( this );
			}
		}

		public void Simulate(float deltaTime)
		{
            if (deltaTime <= 0)
                return;

			RenderManager?.EndRender();
			SceneManager.RemoveEntities();

			SynchronizedClock.Tick( deltaTime );
			TickScheduler.Tick();
			RenderManager?.BeginRender();

			//NetworkManager?.ReceiveMessages();

			//var targetFixedTime = SynchronizedClock.Time * 1000;
			//if( ClientTickScheduler != null )
			//{
			//	targetFixedTime += SynchronizedClock.Rtt / 2 * 1000 + Configuration.SimulationDeltaTime;

			//	if( FixedTime < ClientTickScheduler.StartFixedTime )
			//	{
			//		Context.Log( "Reset fixed time => {0}", ClientTickScheduler.StartFixedTime );
			//		FixedTime = ClientTickScheduler.StartFixedTime.Value;
			//	}
			//}

			//if( ClientTickScheduler == null || ClientTickScheduler.StartFixedTime != null )
			//{
			//	while( FixedTime + Configuration.SimulationDeltaTime <= targetFixedTime )
			//	{
			//		FixedTime += Configuration.SimulationDeltaTime;

			//		//Context.Log( "Before Tick targetFixedTime: {0}, fixedTime: {1}", targetFixedTime, FixedTime );

			//		TickScheduler.Tick();
			//		EventDispatcher.Invoke();

			//		NetworkManager?.SendMessages();
			//	}

			//	InterpolationManager?.Interpolate();
			//}
		}
	}
}
