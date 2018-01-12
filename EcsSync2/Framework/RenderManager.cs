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
		bool? m_localDeltaTimeDilation;
		bool? m_remoteDeltaTimeDilation;
		float? m_interpolationDelayCheckTime;

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

				localTime = CalculateNextTickMs(
					ref m_localDeltaTimeOdd,
					ref m_localDeltaTimeDilation,
					Simulator.ClientTickScheduler.PredictionTickTime - Configuration.SimulationDeltaTime,
					m_tickContext.LocalTime );

				//InterpolationDelay = (uint)Math.Round( Configuration.SimulationDeltaTime + Simulator.SynchronizedClock.RttStdErr * 1000 );
				remoteTime = CalculateNextTickMs(
					ref m_remoteDeltaTimeOdd,
					ref m_remoteDeltaTimeDilation,
					Simulator.ClientTickScheduler.SyncTickTime - InterpolationDelay,
					m_tickContext.RemoteTime );

				//if( localTime >= Simulator.ClientTickScheduler.PredictionTickTime )
				//	Simulator.Context.LogWarning( "Extrapolate predtiction {0} > {1}", localTime, Simulator.ClientTickScheduler.PredictionTickTime );

				if( remoteTime + Configuration.SimulationDeltaTime >= Simulator.ClientTickScheduler.SyncTickTime )
				{
					m_interpolationDelayCheckTime = null;

					var newInterpolationDelay = remoteTime + Configuration.SimulationDeltaTime * 2 - Simulator.ClientTickScheduler.SyncTickTime;
					if( newInterpolationDelay > InterpolationDelay )
					{
						InterpolationDelay = newInterpolationDelay;
					}
				}
				else if( InterpolationDelay > Configuration.SimulationDeltaTime )
				{
					if( remoteTime + InterpolationDelay <= Simulator.ClientTickScheduler.SyncTickTime )
					{
						if( m_interpolationDelayCheckTime == null )
						{
							m_interpolationDelayCheckTime = Simulator.SynchronizedClock.LocalTime;
							//Simulator.Context.Log( "Start decrease IND {0}", m_interpolationDelayCheckTime );
						}
						else if( Simulator.SynchronizedClock.LocalTime - m_interpolationDelayCheckTime.Value > 1 )
						{
							InterpolationDelay = Math.Max( InterpolationDelay - 10, Configuration.SimulationDeltaTime );
							//Simulator.Context.Log( "Complete decrease IND {0} / {1}", m_interpolationDelayCheckTime, InterpolationDelay );
							m_interpolationDelayCheckTime = null;

						}
					}
					else
					{
						//Simulator.Context.Log( "Reset decrease IND {0}", m_interpolationDelayCheckTime );
						m_interpolationDelayCheckTime = null;
					}
				}

				if( remoteTime > Simulator.ClientTickScheduler.SyncTickTime )
				{
					Simulator.Context.LogWarning( "Extrapolate sync {0} > {1}, Dilation {2}, IND: {3}",
						remoteTime, Simulator.ClientTickScheduler.SyncTickTime, m_remoteDeltaTimeDilation, InterpolationDelay );
				}
				//else if( InterpolationDelay > targetInterpolationDelay )
				//{
				//	if( m_remoteDeltaTimeDilation != false )
				//		InterpolationDelay -= 10;
				//}
				//else if( Simulator.ClientTickScheduler.SyncTickTime - remoteTime > ( Simulator.SynchronizedClock.Rtt + Simulator.SynchronizedClock.RttStdErr ) * 1000 / 2
				//	&& InterpolationDelay > Configuration.SimulationDeltaTime + Simulator.SynchronizedClock.RttStdErr * 1000 / 2 )
				//{
				//	InterpolationDelay -= 10;
				//}
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

		uint CalculateNextTickMs(ref float deltaTimeOdd, ref bool? deltaTimeDilation, uint targetMs, uint currentMs)
		{
			var deltaTime = Simulator.SynchronizedClock.DeltaTime;

			if( currentMs + deltaTime * 1000 > targetMs )
			{
				deltaTime *= 0.9f;
				deltaTimeDilation = false;
			}
			else if( currentMs + deltaTime * 1000 < targetMs - Configuration.SimulationDeltaTime )
			{
				deltaTime /= 0.9f;
				deltaTimeDilation = true;
			}
			else
				deltaTimeDilation = null;

			deltaTimeOdd += deltaTime;

			var deltaMs = Math.Max( 1, (uint)Math.Floor( deltaTimeOdd * 1000 ) );
			deltaTimeOdd -= deltaMs / 1000f;

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

		public uint InterpolationDelay { get; private set; } = Configuration.SimulationDeltaTime;
	}
}
