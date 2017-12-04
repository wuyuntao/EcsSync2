using System;

namespace EcsSync2
{
	public class InterpolationManager : SimulatorComponent
	{
		TickScheduler.TickContext m_context = new TickScheduler.TickContext( TickScheduler.TickContextType.Interpolation, 0 );

		public InterpolationManager(Simulator simulator)
			: base( simulator )
		{
		}

		internal void BeginInterpolate()
		{
			var time = (uint)Math.Round( Math.Max( 0f, Simulator.SynchronizedClock.Time * 1000f - Configuration.SimulationDeltaTime ) );
			if( time <= m_context.Time )
				return;

			m_context = new TickScheduler.TickContext( TickScheduler.TickContextType.Interpolation, time );

			Simulator.TickScheduler.EnterContext( m_context );

			foreach( var component in Simulator.TickScheduler.Components )
				component.Interpolate();
		}

		internal void EndInterpolate()
		{
			Simulator.TickScheduler.LeaveContext();
		}

		internal TickScheduler.TickContext? CurrentContext => m_context;

		public uint InterpolationDelay { get; private set; } = 50;
	}
}
