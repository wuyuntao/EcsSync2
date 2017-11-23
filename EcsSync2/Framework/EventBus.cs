﻿using System.Collections.Generic;

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

		internal void EnqueueEvent(uint time, Event @event)
		{
			//Simulator.Context.Log( "EnqueueEvent {0}ms {1}", time, @event );

			@event.Retain();
			m_events.Enqueue( @event );

			var frame = EnsureFrame( time );
			@event.Retain();
			frame.Events.Add( @event );
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
			var f = EnsureFrame( time );
			m_deltaSyncFrames.Remove( time );
			return f;
		}
	}
}
