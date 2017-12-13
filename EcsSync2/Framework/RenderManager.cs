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

		internal void BeginRender()
		{
			uint localTime;
			uint remoteTime;
			if( Simulator.ClientTickScheduler != null )
			{
				if( Simulator.ClientTickScheduler.FullSyncTime == null )
					return;

				localTime = RoundToMs( Simulator.SynchronizedClock.Time + Simulator.SynchronizedClock.Rtt / 2 );
				remoteTime = RoundToMs( Simulator.SynchronizedClock.Time - Simulator.SynchronizedClock.Rtt / 2 - Configuration.SimulationDeltaTime / 1000f - InterpolationDelay / 1000f );
			}
			else
			{
				localTime = remoteTime = RoundToMs( Simulator.SynchronizedClock.Time - Configuration.SimulationDeltaTime / 1000f );
			}
			if( localTime <= m_tickContext.LocalTime || remoteTime <= m_tickContext.RemoteTime )
			{
				//Simulator.Context.LogWarning( $"Skip rendering {localTime} < {m_tickContext.LocalTime} || {remoteTime} < {m_tickContext.RemoteTime}" );
				return;
			}

			//Simulator.Context.Log( "Render lt: {0}, llt: {1}, ldt: {2}, dt: {3:f2}, rt: {4}, lrt: {5}, rdt: {6}, cts: {7}",
			//	localTime, m_tickContext.LocalTime, localTime - m_tickContext.LocalTime,
			//	Simulator.SynchronizedClock.DeltaTime * 1000f,
			//	remoteTime, m_tickContext.RemoteTime, remoteTime - m_tickContext.RemoteTime,
			//	Simulator.ClientTickScheduler.m_predictionTickContext.LocalTime );

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
