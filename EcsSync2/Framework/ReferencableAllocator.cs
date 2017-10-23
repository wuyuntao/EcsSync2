using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public interface IReferencable
	{
		void Reset();

		IReferenceCounter ReferenceCounter { get; }
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

		int ReferencedCount { get; }
	}

	public interface IReferenceCounter<T> : IReferenceCounter
		where T : IReferencable
	{
		T Value { get; }
	}

	public class ReferencableAllocator
	{
		Dictionary<Type, Queue<IReferenceCounter>> m_unreferenced = new Dictionary<Type, Queue<IReferenceCounter>>();
		Dictionary<Type, Queue<IReferenceCounter>> m_referenced = new Dictionary<Type, Queue<IReferenceCounter>>();

		public IReferenceCounter<T> Allocate<T>()
			where T : class, IReferencable, new()
		{
			var unreferenced = EnsureQueue( m_unreferenced, typeof( T ) );
			var counter = EnsureCounter<T>( unreferenced );
			var referenced = EnsureQueue( m_referenced, typeof( T ) );
			referenced.Enqueue( counter );
			return counter;
		}

		Queue<IReferenceCounter> EnsureQueue(Dictionary<Type, Queue<IReferenceCounter>> queues, Type type)
		{
			if( !queues.TryGetValue( type, out Queue<IReferenceCounter> queue ) )
			{
				queue = new Queue<IReferenceCounter>();
				queues.Add( type, queue );
			}
			return queue;
		}

		IReferenceCounter<T> EnsureCounter<T>(Queue<IReferenceCounter> queue)
			where T : class, IReferencable, new()
		{
			IReferenceCounter<T> counter;
			if( queue.Count > 0 )
				counter = (IReferenceCounter<T>)queue.Dequeue();
			else
				counter = new ReferencableCounter<T>( this );

			return counter;
		}

		#region ReferencableCounter

		class ReferencableCounter<T> : IReferenceCounter<T>
			where T : class, IReferencable, new()
		{
			readonly ReferencableAllocator m_allocator;
			readonly T m_value;
			int m_referencedCount;

			public ReferencableCounter(ReferencableAllocator allocator)
			{
				m_value = new T();
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

					var unreferenced = m_allocator.EnsureQueue( m_allocator.m_unreferenced, typeof( T ) );
					unreferenced.Enqueue( (IReferenceCounter)this );
				}
			}

			public T Value => m_value;

			public int ReferencedCount => m_referencedCount;
		}

		#endregion
	}
}
