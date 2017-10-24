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
		Timepoint[] m_points;
		int m_firstIndex = -1;
		int m_lastIndex = -1;

		public Timeline(Simulator simulator, int capacity)
		{
			m_simulator = simulator;
			m_points = new Timepoint[capacity];
			for( int i = 0; i < capacity; i++ )
				m_points[i] = new Timepoint();
		}

		public Timeline(Simulator simulator)
			: this( simulator, Settings.TimelineDefaultCapacity )
		{
		}

		public void AddPoint(uint time, Message snapshot)
		{
			snapshot.Retain();

			if( m_firstIndex < 0 )
			{
				var first = m_points[0];
				first.Time = time;
				first.Snapshot = snapshot;
				m_firstIndex = m_lastIndex = 0;
				return;
			}

			var last = m_points[m_lastIndex];
			if( last.Time == time )
			{
				last.Snapshot.Release();
				last.Snapshot = snapshot;
				return;
			}

			var index = ( m_lastIndex + 1 ) % m_points.Length;
			if( index == m_firstIndex )
			{
				m_points[m_firstIndex].Release();
				m_firstIndex = ( m_firstIndex + 1 ) % m_points.Length;
			}

			var current = m_points[index];
			current.Time = time;
			current.Snapshot = snapshot;
		}
	}
}
