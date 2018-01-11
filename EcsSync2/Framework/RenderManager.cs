using System;
using EcsSync2.Fps;
using System.Collections.Generic;

namespace EcsSync2
{
	public class RenderManager : SimulatorComponent
	{
		public interface IContext
		{
			void CreateEntityPawn(Entity entity);

			void DestroyEntityPawn(Entity entity);
		}

		IContext m_context;
		TickScheduler.TickContext m_tickContext = new TickScheduler.TickContext( TickScheduler.TickContextType.Interpolation, 0, 0 );
		List<Entity> m_newEntities = new List<Entity>();
		List<Entity> m_destroyedEntities = new List<Entity>();
		List<Renderer> m_renderers = new List<Renderer>();
		float m_localDeltaTimeOdd;
		float m_remoteDeltaTimeOdd;

		public RenderManager(Simulator simulator)
			: base( simulator )
		{
			m_context = (IContext)Simulator.Context;
			simulator.SceneManager.OnSceneLoaded += SceneManager_OnSceneLoaded;
		}

		void SceneManager_OnSceneLoaded(Scene scene)
		{
			scene.OnEntityCreated += Scene_OnEntityCreated;
			scene.OnEntityRemoved += Scene_OnEntityRemoved;
		}

		void Scene_OnEntityCreated(Scene scene, Entity entity)
		{
			m_newEntities.Add( entity );
		}

		void Scene_OnEntityRemoved(Scene scene, Entity entity)
		{
			m_destroyedEntities.Add( entity );
		}

		internal void AddRenderer(Renderer renderer)
		{
			m_renderers.Add( renderer );
		}

		internal void CreateTickContext(uint time)
		{
			m_tickContext = new TickScheduler.TickContext( TickScheduler.TickContextType.Interpolation, time );
		}

		internal void BeginRender()
		{
			var deltaTime = Configuration.SimulationDeltaTime / 1000f;
			uint localTime;
			uint remoteTime;
			if( Simulator.ClientTickScheduler != null )
			{
				if( Simulator.ClientTickScheduler.FullSyncTime == null )
					return;

				localTime = CalculateTime( ref m_localDeltaTimeOdd, Simulator.ClientTickScheduler.PredictionTickTime - Configuration.SimulationDeltaTime, m_tickContext.LocalTime );
				remoteTime = CalculateTime( ref m_remoteDeltaTimeOdd, Simulator.ClientTickScheduler.SyncTickTime - InterpolationDelay, m_tickContext.RemoteTime );

				//if( localTime >= Simulator.ClientTickScheduler.PredictionTickTime )
				//	Simulator.Context.LogWarning( "Extrapolate predtiction {0} > {1}", localTime, Simulator.ClientTickScheduler.PredictionTickTime );

				//if( remoteTime >= Simulator.ClientTickScheduler.SyncTickTime )
				//	Simulator.Context.LogWarning( "Extrapolate sync {0} > {1}", remoteTime, Simulator.ClientTickScheduler.SyncTickTime );
			}
			else
			{
				localTime = remoteTime = RoundToMs( Simulator.SynchronizedClock.Time - deltaTime / 1000f );
			}

			if( localTime <= m_tickContext.LocalTime || remoteTime <= m_tickContext.RemoteTime )
			{
				Simulator.Context.LogWarning( $"Skip rendering {localTime} < {m_tickContext.LocalTime} || {remoteTime} < {m_tickContext.RemoteTime}" );
				return;
			}

			//if( Simulator.ClientTickScheduler != null )
			//{
			//	Simulator.Context.Log( "Render: l {0}, r {1}. Tick: l {2}, r {3}, a {4}",
			//		localTime,
			//		remoteTime,
			//		Simulator.ClientTickScheduler.PredictionTickTime,
			//		Simulator.ClientTickScheduler.SyncTickTime,
			//		Simulator.ClientTickScheduler.CommandAdvanceTime );
			//}

			m_tickContext = new TickScheduler.TickContext( TickScheduler.TickContextType.Interpolation, localTime, remoteTime );
			Simulator.TickScheduler.EnterContext( m_tickContext );

			foreach( var entity in m_newEntities )
				m_context.CreateEntityPawn( entity );

			var hasRendererDestroyed = false;
			foreach( var renderer in m_renderers )
			{
				renderer.Update();
				hasRendererDestroyed |= renderer.IsDestroyed;
			}

			foreach( var entity in m_destroyedEntities )
				m_context.DestroyEntityPawn( entity );

			// Cleanup
			m_newEntities.Clear();
			if( hasRendererDestroyed )
				m_renderers.RemoveAll( r => r.IsDestroyed );
			m_destroyedEntities.Clear();
		}

		uint CalculateTime(ref float remainingDeltaTime, uint targetMs, uint currentMs)
		{
			var deltaTime = Simulator.SynchronizedClock.DeltaTime;

			if( currentMs + deltaTime * 1000 > targetMs )
				deltaTime *= 0.9f;
			else if( currentMs + deltaTime * 1000 < targetMs - (float)InterpolationDelay )
				deltaTime /= 0.9f;

			remainingDeltaTime += deltaTime;

			var deltaMs = (uint)Math.Floor( remainingDeltaTime * 1000 );
			remainingDeltaTime -= deltaMs / 1000f;

			return currentMs + deltaMs;
		}

		internal void EndRender()
		{
			Simulator.TickScheduler.LeaveContext();
		}

		static uint RoundToMs(float seconds)
		{
			return (uint)Math.Round( Math.Max( 0f, seconds * 1000 ) );
		}

		internal TickScheduler.TickContext? CurrentContext => m_tickContext;

		public uint InterpolationDelay { get; private set; } = 50;
	}
}
