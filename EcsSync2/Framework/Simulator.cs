using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public class Simulator
	{
		public interface IContext : InputManager.IContext
		{
		}

		List<SimulatorComponent> m_components = new List<SimulatorComponent>();

		public IContext Context { get; }
		public bool IsServer { get; }
		public bool IsClient { get; }
		public Random Random { get; }
		public ulong? LocalUserId { get; }

		public ReferencableAllocator ReferencableAllocator { get; }
		public SynchronizedClock SynchronizedClock { get; }
		public InputManager InputManager { get; }
		public CommandDispatcher CommandDispatcher { get; }

		public uint FixedTime { get; private set; }
		public uint FixedDeltaTime => Settings.FixedDeltaTime;

		public uint Time => (uint)Math.Round( SynchronizedClock.Time * 1000 );
		public uint DeltaTime => (uint)Math.Round( SynchronizedClock.DeltaTime * 1000 );


		public Simulator(IContext context, bool isServer, bool isClient, int? randomSeed, ulong? localUserId)
		{
			Context = context;
			IsServer = isServer;
			IsClient = isClient;
			Random = randomSeed != null ? new Random( randomSeed.Value ) : null;
			LocalUserId = localUserId;

			ReferencableAllocator = new ReferencableAllocator();
			SynchronizedClock = new SynchronizedClock();

			if( isClient )
				InputManager = AddComponent<InputManager>();

			CommandDispatcher = AddComponent<CommandDispatcher>();
		}

		T AddComponent<T>()
			where T : SimulatorComponent, new()
		{
			if( SynchronizedClock.Time > 0 )
				throw new InvalidOperationException( "Cannot add manager after simulator started" );

			var m = new T();
			m.OnInitialize( this );
			m_components.Add( m );
			m.OnStart();
			return m;
		}

		public void Simulate(float deltaTime)
		{
			SynchronizedClock.Tick( deltaTime );

			foreach( var m in m_components )
				m.OnUpdate();

			while( FixedTime <= Time + FixedDeltaTime )
			{
				FixedTime += FixedDeltaTime;

				foreach( var m in m_components )
					m.OnFixedUpdate();
			}

			foreach( var m in m_components )
				m.OnLateUpdate();
		}
	}
}
