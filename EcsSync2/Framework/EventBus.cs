using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public class EventBus : SimulatorComponent
	{
		Queue<Event> m_events = new Queue<Event>();
		Queue<Event> m_unsyncEvents = new Queue<Event>();

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

				m_unsyncEvents.Enqueue( @event );
			}
		}

		void OnDispatchEvent(Event @event)
		{
		}

		internal void EnqueueEvent(Event @event)
		{
			m_events.Enqueue( @event );
			m_unsyncEvents.Enqueue( @event );

			@event.Retain();
			@event.Retain();
		}

		internal IEnumerable<Event> FetchUnsyncedEvents()
		{
			while( m_unsyncEvents.Count > 0 )
				yield return m_unsyncEvents.Dequeue();
		}
	}
}
