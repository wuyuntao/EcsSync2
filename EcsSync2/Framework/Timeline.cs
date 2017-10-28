using System;
using System.Diagnostics;

namespace EcsSync2
{
	class Timepoint
	{
		public uint Time;
		public Snapshot Snapshot;
	}

	class Timeline
	{
		Simulator m_simulator;

		Timepoint[] m_points;
		int m_head;
		int m_tail;
		int m_count;

		public Timeline(Simulator simulator, int capacity)
		{
			m_simulator = simulator;

			m_points = new Timepoint[capacity];
			FillPoints( m_points, 0, capacity );
		}

		public Timeline(Simulator simulator)
			: this( simulator, Configuration.TimelineDefaultCapacity )
		{
		}

		public void AddPoint(uint time, Snapshot snapshot)
		{
			Debug.Assert( ( time % Configuration.SimulationDeltaTime ) == 0 );

			var lastPoint = LastPoint;
			if( lastPoint != null && time < lastPoint.Time )
				throw new InvalidOperationException( $"Cannot add point before last: {time} < {lastPoint.Time}" );

			Timepoint point;
			if( lastPoint != null && lastPoint.Time == time )
				point = lastPoint;
			else
				point = EnqueuePoint( time );

			point.Snapshot?.Release();
			point.Snapshot = snapshot;
			point.Snapshot.Retain();
		}

		public Snapshot GetPoint(uint time)
		{
			Debug.Assert( ( time % Configuration.SimulationDeltaTime ) == 0 );

			if( m_count == 0 )
				return null;

			for( int i = m_tail - 1; i >= m_head; i-- )
			{
				var point = m_points[i % m_points.Length];
				if( time >= point.Time )
					return point.Snapshot;
			}

			return null;
		}

		public Snapshot InterpolatePoint(uint time)
		{
			if( m_count == 0 )
				return null;

			for( int i = m_tail - 1; i >= m_head; i-- )
			{
				var prevPoint = m_points[i % m_points.Length];
				if( time < prevPoint.Time )
					continue;

				// Equals
				if( time == prevPoint.Time )
					return prevPoint.Snapshot.Clone();

				// Extrapolation
				if( i == m_tail - 1 )
					return prevPoint.Snapshot.Extrapolate(prevPoint.Time, time);

				// Interpolation
				var nextPoint = m_points[( i + 1 ) % m_points.Length];
				return prevPoint.Snapshot.Interpolate(prevPoint.Time, nextPoint.Snapshot, nextPoint.Time, time);
			}

			return null;
		}

		public void Clear()
		{
			m_head = m_tail = m_count = 0;
		}

		void FillPoints(Timepoint[] points, int offset, int size)
		{
			for( int i = offset; i < size; i++ )
				points[i] = new Timepoint();
		}

		Timepoint EnqueuePoint(uint time)
		{
			EnsureCapacity();

			var point = m_points[m_tail % m_points.Length];
			point.Time = time;
			m_tail++;
			m_count++;
			return point;
		}

		void EnsureCapacity()
		{
			var points = new Timepoint[m_points.Length * 2];
			var tail = m_tail % m_points.Length;
			var head = m_head % m_points.Length;

			if( tail > head )
			{
				FillPoints( points, 0, head );                              // [0, head)
				Array.Copy( m_points, head, points, head, tail - head );    // [head, tail)
				FillPoints( points, tail, points.Length - tail );           // [tail, capacity * 2)
			}
			else if( tail < head )
			{
				FillPoints( points, 0, head );                                          // [0, head)
				Array.Copy( m_points, head, points, head, m_points.Length - head );     // [head, capacity)
				Array.Copy( m_points, 0, points, m_points.Length, tail );               // [capacity, capacity + tail)
				FillPoints( points, m_points.Length + tail, m_points.Length - tail );   // [capacity + tail, capacity * 2)
			}
			else
			{
				Array.Copy( m_points, points, m_points.Length );            // [0, capacity)
				FillPoints( points, m_points.Length, m_points.Length );     // [capacity, capacity * 2)
			}

			m_points = points;
			m_head = m_head % m_points.Length;
			m_tail = m_tail % m_points.Length;
		}

		public Timepoint FirstPoint => m_count > 0 ? m_points[m_head % m_points.Length] : null;

		public Timepoint LastPoint => m_count > 0 ? m_points[( m_tail - 1 ) % m_points.Length] : null;
	}
}
