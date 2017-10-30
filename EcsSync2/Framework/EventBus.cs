using System.Collections.Generic;

namespace EcsSync2
{
	public class EventBus : SimulatorComponent
	{
		Queue<Event> m_events = new Queue<Event>();

		public EventBus(Simulator simulator)
			: base( simulator )
		{
		}

		internal void DispatchEvents()
		{
			while( m_events.Count > 0 )
			{
				var @event = m_events.Dequeue();
				OnDispatchEvent( @event );
				//@event.Release();
			}
		}

		void OnDispatchEvent(Event @event)
		{
		}

		internal void EnqueueEvent(Event @event)
		{
			m_events.Enqueue( @event );

			@event.Retain();
		}
	}
}
