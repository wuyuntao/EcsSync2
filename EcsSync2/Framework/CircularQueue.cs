namespace EcsSync2
{
	class CircularQueue<T>
		where T : class, new()
	{
		T[] m_values;
		int m_firstIndex = -1;
		int m_lastIndex = -1;
		int m_count = 0;

		public CircularQueue(int capacity)
		{
			m_values = new T[capacity];
			for( int i = 0; i < capacity; i++ )
				m_values[i] = new T();
		}

		public T Enqueue()
		{
			if( m_count == 0 )
			{
				m_firstIndex = m_lastIndex = 0;
			}
			else
			{
				m_lastIndex = ( m_lastIndex + 1 ) % m_values.Length;
				if( m_lastIndex == m_firstIndex )
					m_firstIndex = ( m_firstIndex + 1 ) % m_values.Length;
				else
					m_count++;
			}

			return m_values[m_lastIndex];
		}

		public T Dequeue()
		{
			if( m_count == 0 )
				return null;

			var value = m_values[m_firstIndex];
			if( m_firstIndex == m_lastIndex )
			{
				m_firstIndex = m_lastIndex = -1;
				m_count = 0;
			}
			else
			{
				m_firstIndex++;
				m_count--;
			}

			return value;
		}

		public void Clear()
		{
			m_firstIndex = m_lastIndex = -1;
			m_count = 0;
		}

		public T First => m_firstIndex >= 0 ? m_values[m_firstIndex] : null;

		public T Last => m_lastIndex >= 0 ? m_values[m_lastIndex] : null;

		public int Count => m_count;

		public int Capacity => m_values.Length;
	}
}
