﻿using System;
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
		TickScheduler.TickContext m_tickContext = new TickScheduler.TickContext( TickScheduler.TickContextType.Interpolation, 0 );
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
			var time = (uint)Math.Round( Math.Max( 0f, Simulator.SynchronizedClock.Time * 1000f - Configuration.SimulationDeltaTime ) );
			if( time <= m_tickContext.Time )
				return;

			//Simulator.Context.Log( "Render {0}, {1}", time, Simulator.StandaloneTickScheduler.Time );

			m_tickContext = new TickScheduler.TickContext( TickScheduler.TickContextType.Interpolation, time );
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

		internal TickScheduler.TickContext? CurrentContext => m_tickContext;

		public uint InterpolationDelay { get; private set; } = 50;
	}
}
