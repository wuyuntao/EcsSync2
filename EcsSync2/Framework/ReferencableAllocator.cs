using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public interface IReferencable
	{
		void Reset();

		IReferenceCounter ReferenceCounter { get; set; }
	}

	public static class ReferencableExtensions
	{
		public static void Retain(this object obj)
		{
			if( obj is IReferencable referencable )
				referencable.ReferenceCounter.Retain();
		}

		public static void Release(this object obj)
		{
			if( obj is IReferencable referencable )
				referencable.ReferenceCounter.Release();
		}
	}

	public interface IReferenceCounter
	{
		void Retain();

		void Release();

		T Allocate<T>() where T : class, IReferencable, new();

		T Allocate<T>(T value) where T : class, IReferencable, new();

		int ReferencedCount { get; }
	}

	public interface IReferenceCounter<T> : IReferenceCounter
		where T : IReferencable
	{
		T Value { get; }
	}

	public class ReferencableAllocator
	{
		int m_maxCapacity;
		Dictionary<Type, ReferencableCounterPool> m_pools = new Dictionary<Type, ReferencableCounterPool>();

		public ReferencableAllocator(int maxCapacity = 100)
		{
			m_maxCapacity = maxCapacity;
		}

		public T Allocate<T>()
			where T : class, IReferencable, new()
		{
			return EnsurePool<T>().Allocate<T>().Value;
		}

		public T Allocate<T>(T value)
			where T : class, IReferencable, new()
		{
			return EnsurePool<T>().Allocate( value ).Value;
		}

		public void Clear()
		{
			m_pools.Clear();
		}

		ReferencableCounterPool EnsurePool<T>()
			where T : class, IReferencable, new()
		{
			if( !m_pools.TryGetValue( typeof( T ), out ReferencableCounterPool pool ) )
			{
				pool = new ReferencableCounterPool( this );
				m_pools.Add( typeof( T ), pool );
			}

			return pool;
		}

		#region ReferencableCounterPool

		class ReferencableCounterPool
		{
			readonly List<IReferenceCounter> m_counters;
			readonly Queue<int> m_unreferenced;

			public ReferencableCounterPool(ReferencableAllocator allocator, int initialCapacity = 16)
			{
				Allocator = allocator;
				m_counters = new List<IReferenceCounter>( initialCapacity );
				m_unreferenced = new Queue<int>( initialCapacity );
			}

			public IReferenceCounter<T> Allocate<T>()
				where T : class, IReferencable, new()
			{
				if( m_unreferenced.Count > 0 )
				{
					var index = m_unreferenced.Dequeue();
					return (IReferenceCounter<T>)m_counters[index];
				}
				else if( m_counters.Count < Allocator.m_maxCapacity )
				{
					var counter = new ReferencableCounter<T>( this, m_counters.Count );
					m_counters.Add( counter );
					return counter;
				}
				else
				{
					return new ReferencableCounter<T>( this, -1 );
				}
			}

			public IReferenceCounter<T> Allocate<T>(T value)
				where T : class, IReferencable, new()
			{
				if( m_counters.Count < Allocator.m_maxCapacity )
				{
					var counter = new ReferencableCounter<T>( this, m_counters.Count, value );
					m_counters.Add( counter );
					return counter;
				}
				else
				{
					return new ReferencableCounter<T>( this, -1, value );
				}
			}

			public void Release(int index)
			{
				m_unreferenced.Enqueue( index );
			}

			public ReferencableAllocator Allocator { get; private set; }
		}

		#endregion

		#region ReferencableCounter

		class ReferencableCounter<T> : IReferenceCounter<T>
			where T : class, IReferencable, new()
		{
			readonly ReferencableCounterPool m_pool;
			readonly int m_index;
			readonly T m_value;
			int m_referencedCount;

			public ReferencableCounter(ReferencableCounterPool pool, int index, T value)
			{
				if( value.ReferenceCounter != null )
					throw new InvalidOperationException( "Already allocated" );

				m_pool = pool;
				m_index = index;
				m_value = value;
				m_value.ReferenceCounter = this;
				m_referencedCount = 1;
			}

			public ReferencableCounter(ReferencableCounterPool pool, int index)
				: this( pool, index, new T() )
			{ }

			public void Retain()
			{
				m_referencedCount++;
			}

			public void Release()
			{
				if( m_referencedCount == 0 )
					throw new InvalidOperationException( "Already released" );

				if( --m_referencedCount == 0 )
				{
					m_value.Reset();

					if( m_index >= 0 )
						m_pool.Release( m_index );
				}
			}

			public T1 Allocate<T1>()
				where T1 : class, IReferencable, new()
			{
				return m_pool.Allocator.Allocate<T1>();
			}

			public T1 Allocate<T1>(T1 value)
				where T1 : class, IReferencable, new()
			{
				return m_pool.Allocator.Allocate<T1>( value );
			}

			public T Value => m_value;

			public int ReferencedCount => m_referencedCount;
		}

		#endregion
	}
}
