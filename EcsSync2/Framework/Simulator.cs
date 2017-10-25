using System;

namespace EcsSync2
{
	public class Simulator
	{
		public interface IContext : InputManager.IContext
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
		public ComponentScheduler ComponentScheduler { get; }
		public EventBus EventBus { get; }
		public InterpolationManager InterpolationManager { get; }

		public uint FixedTime { get; private set; }
		public uint FixedDeltaTime => Settings.SimulationDeltaTime;

		public uint Time => (uint)Math.Round( SynchronizedClock.Time * 1000 );
		public uint DeltaTime => (uint)Math.Round( SynchronizedClock.DeltaTime * 1000 );

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
				ComponentScheduler = new ServerComponentScheduler( this );
			else
				ComponentScheduler = new ClientComponentScheduler( this );
		}

		public void Simulate(float deltaTime)
		{
			SynchronizedClock.Tick( deltaTime );

			while( FixedTime <= Time + FixedDeltaTime )
			{
				FixedTime += FixedDeltaTime;

				InputManager?.SetInput();
				InputManager?.EnqueueCommands();

				ComponentScheduler.FixedUpdate();
				EventBus.DispatchEvents();

				InputManager?.ResetInput();
			}

			InterpolationManager.Interpolate();
		}
	}
}
