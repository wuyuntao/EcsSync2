using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public class EventBus : SimulatorComponent
	{
		Queue<Event> m_events = new Queue<Event>();
		SortedList<uint, DeltaSyncFrame> m_deltaSyncFrames = new SortedList<uint, DeltaSyncFrame>();

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
				@event.Release();
			}
		}

		void OnDispatchEvent(Event @event)
		{
		}

		internal void EnqueueEvent(Event @event)
		{
			Simulator.Context.Log( "EnqueueEvent {0}", @event );

			m_events.Enqueue( @event );

			var frame = EnsureFrame( @event.Time );
			frame.Events.Add( @event );

			@event.Retain();
			@event.Retain();
		}

		DeltaSyncFrame EnsureFrame(uint time)
		{
			if( !m_deltaSyncFrames.TryGetValue( time, out DeltaSyncFrame frame ) )
			{
				frame = Simulator.ReferencableAllocator.Allocate<DeltaSyncFrame>();
				frame.Time = time;
				frame.Retain();

				m_deltaSyncFrames.Add( time, frame );
			}
			return frame;
		}

		internal DeltaSyncFrame FetchEvents(uint time)
		{
			return EnsureFrame( time );
		}
	}
}
