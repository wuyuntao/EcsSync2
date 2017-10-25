namespace EcsSync2
{
	class CircularQueue<T>
		where T : class, new()
	{
		T[] m_values;
		int m_head = 0;
		int m_tail = 0;
		int m_count = 0;

		public CircularQueue(int capacity)
		{
			m_values = new T[capacity];
			for( int i = 0; i < capacity; i++ )
				m_values[i] = new T();
		}

		public T Enqueue()
		{
			// Auto remove head when queue is full
			if( m_values.Length == m_count )
				Dequeue();

			var value = m_values[m_tail];
			m_tail++;
			m_count++;
			return value;
		}

		public T Dequeue()
		{
			if( m_count == 0 )
				return null;

			var value = m_values[m_head];
			m_head++;
			m_count--;
			return value;
		}

		public void Clear()
		{
			m_head = m_tail = m_count = 0;
		}

		public T First => m_count > 0 ? m_values[m_head % m_values.Length] : null;

		public T Last => m_count > 0 ? m_values[( m_tail - 1 ) % m_values.Length] : null;

		public int Count => m_count;

		public int Capacity => m_values.Length;
	}
}
