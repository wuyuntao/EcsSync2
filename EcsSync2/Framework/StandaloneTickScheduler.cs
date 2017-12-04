namespace EcsSync2
{
	public class StandaloneTickScheduler : TickScheduler
	{
		TickContext m_context = new TickContext( TickContextType.Sync, 0 );

		public StandaloneTickScheduler(Simulator simulator)
			: base( simulator )
		{
		}

		internal override void Tick()
		{
			for( int i = 0; i < Configuration.MaxTickCount; i++ )
			{
				var nextTime = ( m_context.Time + Configuration.SimulationDeltaTime ) / 1000f;
				if( Simulator.SynchronizedClock.Time < nextTime )
					break;

				m_context = new TickContext( TickContextType.Sync, m_context.Time + Configuration.SimulationDeltaTime );

				EnterContext( m_context );

				Simulator.InputManager.SetInput();
				Simulator.InputManager.CreateCommands();

				foreach( var commands in Simulator.CommandQueue.Find( m_context.Time ) )
					DispatchCommands( commands );

				FixedUpdate();
				Simulator.EventDispatcher.Dispatch();

				Simulator.CommandQueue.RemoveBefore( m_context.Time );
				Simulator.InputManager.ResetInput();

				LeaveContext();

				//Simulator.Context.Log( "Tick {0}", m_context.Time );
			}
		}
	}
}
