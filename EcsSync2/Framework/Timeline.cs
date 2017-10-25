using System;

namespace EcsSync2
{
	class Timepoint
	{
		public uint Time;
		public Message Snapshot;
	}

	class Timeline
	{
		Simulator m_simulator;
		CircularQueue<Timepoint> m_points;

		public Timeline(Simulator simulator, int capacity)
		{
			m_simulator = simulator;
			m_points = new CircularQueue<Timepoint>( capacity );
		}

		public Timeline(Simulator simulator)
			: this( simulator, Settings.TimelineDefaultCapacity )
		{
		}

		public void AddPoint(uint time, Message snapshot)
		{
			Timepoint point;
			if( m_points.Last != null && m_points.Last.Time == time )
			{
				point = m_points.Last;
			}
			else
			{
				point = m_points.Enqueue();
				point.Time = time;
			}

			point.Snapshot?.Release();
			point.Snapshot = snapshot;
			point.Snapshot.Retain();
		}

		public void Clear()
		{
			m_points.Clear();
		}
	}
}
