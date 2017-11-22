using System;
using System.Diagnostics;

namespace EcsSync2
{
	class Timepoint : Referencable
	{
		public uint Time;

		public ComponentSnapshot Snapshot;

		protected override void OnReset()
		{
			Time = 0;
			Snapshot?.Release();

			base.OnReset();
		}
	}

	class Timeline
	{
		TickScheduler.TickContextType m_type;
		ReferencableAllocator m_allocator;
		Timepoint[] m_points;
		int m_head;
		int m_count;

		public Timeline(ReferencableAllocator allocator, TickScheduler.TickContextType type, int capacity)
		{
			m_allocator = allocator;
			m_type = type;
			m_points = new Timepoint[capacity];
		}

		public Timeline(ReferencableAllocator allocator, TickScheduler.TickContextType type)
			: this( allocator, type, Configuration.TimelineDefaultCapacity )
		{
		}

		public override string ToString()
		{
			if( m_count > 0 )
				return $"{m_type} ({FirstPoint.Time} -> {LastPoint.Time})";
			else
				return $"{m_type} (Empty)";
		}

		public bool Add(uint time, ComponentSnapshot snapshot)
		{
			Debug.Assert( ( time % Configuration.SimulationDeltaTime ) == 0 );

			var lastPoint = LastPoint;
			if( lastPoint != null && time < lastPoint.Time )
				throw new InvalidOperationException( $"{this}Cannot add point before last: {time} < {lastPoint.Time}" );

			Timepoint point;
			bool isNewPoint;
			if( lastPoint != null && lastPoint.Time == time )
			{
				point = lastPoint;
				isNewPoint = false;
			}
			else
			{
				point = AllocatePoint();
				point.Time = time;
				isNewPoint = true;
			}

			point.Snapshot?.Release();
			point.Snapshot = snapshot;
			point.Snapshot.Retain();

			return isNewPoint;
		}

		public int RemoveBefore(uint time)
		{
			var count = -1;
			for( int i = m_head; i < m_head + m_count; i++ )
			{
				var point = m_points[i % m_points.Length];
				if( point.Time > time )
					break;

				count++;
			}

			if( count > 0 )
			{
				ReleasePoints( m_head, count );

				m_count -= count;
				m_head = ( m_head + count ) % m_points.Length;
				return count;
			}
			else
				return 0;
		}

		public ComponentSnapshot Find(uint time)
		{
			TryFind( time, out ComponentSnapshot snapshot );
			return snapshot;
		}

		public bool TryFind(uint time, out ComponentSnapshot snapshot)
		{
			snapshot = null;

			if( m_count == 0 )
				return false;

			for( int i = m_head + m_count - 1; i >= m_head; i-- )
			{
				var point = m_points[i % m_points.Length];
				if( time >= point.Time )
				{
					snapshot = point.Snapshot;
					return true;
				}
			}

			return false;
		}

		public ComponentSnapshot Interpolate(uint time)
		{
			if( m_count == 0 )
				return null;

			for( int i = m_head + m_count - 1; i >= m_head; i-- )
			{
				var prevPoint = m_points[i % m_points.Length];
				if( time < prevPoint.Time )
					continue;

				// Equals
				if( time == prevPoint.Time )
					return (ComponentSnapshot)prevPoint.Snapshot.Clone();

				// Extrapolation
				if( i == m_head + m_count - 1 )
					return (ComponentSnapshot)prevPoint.Snapshot.Extrapolate( prevPoint.Time, time );

				// Interpolation
				var nextPoint = m_points[( i + 1 ) % m_points.Length];
				return (ComponentSnapshot)prevPoint.Snapshot.Interpolate( prevPoint.Time, nextPoint.Snapshot, nextPoint.Time, time );
			}

			return null;
		}

		public void Clear()
		{
			ReleasePoints( m_head, m_count );
			m_head = m_count = 0;
		}

		Timepoint AllocatePoint()
		{
			EnsureCapacity();

			var index = ( m_head + m_count ) % m_points.Length;
			Debug.Assert( m_points[index] == null );
			var point = m_allocator.Allocate<Timepoint>();
			m_points[index] = point;
			m_count++;
			return point;
		}

		void ReleasePoints(int offset, int size)
		{
			for( int i = offset; i < offset + size; i++ )
			{
				var index = i % m_points.Length;
				m_points[index].Release();
				m_points[index] = null;
			}
		}

		void EnsureCapacity()
		{
			if( m_count < m_points.Length )
				return;

			var points = new Timepoint[m_points.Length * 2];
			var tail = ( m_head + m_count ) % m_points.Length;
			var head = m_head % m_points.Length;

			if( tail > head )
			{
				Array.Copy( m_points, head, points, head, tail - head );                // [head, tail)
			}
			else if( tail < head )
			{
				Array.Copy( m_points, head, points, head, m_points.Length - head );     // [head, capacity)
				Array.Copy( m_points, 0, points, m_points.Length, tail );               // [capacity, capacity + tail)
			}
			else
			{
				Array.Copy( m_points, points, m_points.Length );                        // [0, capacity)
			}

			m_points = points;
			m_head = m_head % m_points.Length;
		}

		public Timepoint FirstPoint => m_count > 0 ? m_points[m_head % m_points.Length] : null;

		public Timepoint LastPoint => m_count > 0 ? m_points[( m_head + m_count - 1 ) % m_points.Length] : null;

		public int Count => m_count;

		public int Capacity => m_points.Length;
	}
}
