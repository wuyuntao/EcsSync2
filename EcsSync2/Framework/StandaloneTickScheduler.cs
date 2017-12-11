using System;

namespace EcsSync2
{
	public class StandaloneTickScheduler : TickScheduler
	{
		TickContext m_tickContext = new TickContext( TickContextType.Sync, 0 );

		public StandaloneTickScheduler(Simulator simulator)
			: base( simulator )
		{
		}

		internal override void Tick()
		{
			for( int i = 0; i < Configuration.MaxTickCount; i++ )
			{
				var nextTime = ( m_tickContext.Time + Configuration.SimulationDeltaTime ) / 1000f;
				if( Simulator.SynchronizedClock.Time < nextTime )
					break;

				m_tickContext = new TickContext( TickContextType.Sync, m_tickContext.Time + Configuration.SimulationDeltaTime );

				EnterContext( m_tickContext );

				Simulator.InputManager.SetInput();
				Simulator.InputManager.CreateCommands();

				foreach( var commands in Simulator.CommandQueue.Find( m_tickContext.Time ) )
					DispatchCommands( commands );

				FixedUpdate();
				Simulator.EventDispatcher.Dispatch();

				Simulator.CommandQueue.RemoveBefore( m_tickContext.Time );
				Simulator.InputManager.ResetInput();

				LeaveContext();

				//Simulator.Context.Log( "Tick {0}", m_context.Time );
			}

			CleanUpAcknowledgedCommands();
			CleanUpSyncSnapshots();
		}

		void CleanUpAcknowledgedCommands()
		{
			Simulator.CommandQueue.RemoveBefore( Simulator.LocalUserId.Value, m_tickContext.Time );
		}

		void CleanUpSyncSnapshots()
		{
			// 清理冗余的 Sync Timeline
			var expiration = (uint)Math.Round( Simulator.SynchronizedClock.Rtt / 2f * 1000f + Simulator.RenderManager.InterpolationDelay * 2 );
			// TODO 验证 m_syncTickContext.Time > expiration 是否正确
			if( m_tickContext.Time > expiration )
			{
				var context = new TickContext( TickContextType.Sync, m_tickContext.Time - expiration );

				foreach( var component in Components )
					component.RemoveStatesBefore( context );
			}
		}

		public uint Time => m_tickContext.Time;
	}
}
