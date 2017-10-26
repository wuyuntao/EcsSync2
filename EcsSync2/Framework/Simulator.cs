using System;

namespace EcsSync2
{
	public class Simulator
	{
		public interface IContext
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
		public ServerTickScheduler ServerTickScheduler { get; }
		public ClientTickScheduler ClientTickScheduler { get; }
		public EventBus EventBus { get; }
		public InterpolationManager InterpolationManager { get; }

		public uint FixedTime { get; private set; }
		public uint FixedDeltaTime => Settings.SimulationDeltaTime;

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
			EventBus = new EventBus( this );

			if( isClient )
			{
				InputManager = new InputManager( this );
				InterpolationManager = new InterpolationManager( this );
			}

			if( isServer )
				TickScheduler = ServerTickScheduler = new ServerTickScheduler( this );
			else
				TickScheduler = ClientTickScheduler = new ClientTickScheduler( this );
		}

		public void Simulate(float deltaTime)
		{
			SynchronizedClock.Tick( deltaTime );

			while( FixedTime <= SynchronizedClock.Time * 1000 + FixedDeltaTime )
			{
				FixedTime += FixedDeltaTime;

				TickScheduler.Tick();
				EventBus.DispatchEvents();
			}

			InterpolationManager?.Interpolate();
		}
	}
}
