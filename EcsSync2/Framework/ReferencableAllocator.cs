﻿using System;
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

		IReferencable Allocate(Type type);

		int ReferencedCount { get; }

		IReferencable Value { get; }
	}

	public class ReferencableAllocator : SimulatorComponent
	{
		int m_maxCapacity;
		Dictionary<Type, ReferencableCounterPool> m_pools = new Dictionary<Type, ReferencableCounterPool>();

		public ReferencableAllocator(Simulator simulator, int maxCapacity = 100)
			: base( simulator )
		{
			m_maxCapacity = maxCapacity;
		}

		public T Allocate<T>()
			where T : class, IReferencable, new()
		{
			return (T)EnsurePool( typeof( T ) ).Allocate<T>().Value;
		}

		public object Allocate(Type type)
		{
			return EnsurePool( type ).Allocate( type ).Value;
		}

		public T Allocate<T>(T value)
			where T : class, IReferencable, new()
		{
			return (T)EnsurePool( typeof( T ) ).Allocate( value ).Value;
		}

		public void Clear()
		{
			m_pools.Clear();
		}

		ReferencableCounterPool EnsurePool(Type type)
		{
			if( !m_pools.TryGetValue( type, out ReferencableCounterPool pool ) )
			{
				pool = new ReferencableCounterPool( this );
				m_pools.Add( type, pool );
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

			public IReferenceCounter Allocate<T>()
				where T : class, IReferencable, new()
			{
				return Allocate( typeof( T ) );
			}

			public IReferenceCounter Allocate<T>(T value)
				where T : class, IReferencable, new()
			{
				if( m_counters.Count < Allocator.m_maxCapacity )
				{
					var counter = new ReferencableCounter( this, m_counters.Count, value );
					m_counters.Add( counter );
					return counter;
				}
				else
				{
					return new ReferencableCounter( this, -1, value );
				}
			}

			public IReferenceCounter Allocate(Type type)
			{
				if( m_unreferenced.Count > 0 )
				{
					var index = m_unreferenced.Dequeue();
					return m_counters[index];
				}
				else if( m_counters.Count < Allocator.m_maxCapacity )
				{
					var counter = new ReferencableCounter( this, m_counters.Count, (IReferencable)Activator.CreateInstance( type ) );
					m_counters.Add( counter );
					return counter;
				}
				else
				{
					return new ReferencableCounter( this, -1, (IReferencable)Activator.CreateInstance( type ) );
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

		class ReferencableCounter : IReferenceCounter
		{
			readonly ReferencableCounterPool m_pool;
			readonly int m_index;
			readonly IReferencable m_value;
			int m_referencedCount;

			public ReferencableCounter(ReferencableCounterPool pool, int index, IReferencable value)
			{
				if( value.ReferenceCounter != null )
					throw new InvalidOperationException( "Already allocated" );

				m_pool = pool;
				m_index = index;
				m_value = value;
				m_value.ReferenceCounter = this;
				m_referencedCount = 1;
			}

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
				return (T1)m_pool.Allocate<T1>().Value;
			}

			public T1 Allocate<T1>(T1 value)
				where T1 : class, IReferencable, new()
			{
				return (T1)m_pool.Allocate<T1>( value ).Value;
			}

			public IReferencable Allocate(Type type)
			{
				return m_pool.Allocate( type ).Value;
			}

			public IReferencable Value => m_value;

			public int ReferencedCount => m_referencedCount;
		}

		#endregion
	}
}
