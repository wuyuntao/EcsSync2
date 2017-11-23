using MessagePack;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EcsSync2
{
	public interface IReferencable
	{
		void OnAllocate();

		void OnReset();

		IReferenceCounter ReferenceCounter { get; set; }
	}

	public abstract class Referencable : IReferencable
	{
		protected virtual void OnAllocate()
		{
		}

		protected virtual void OnReset()
		{
		}

		#region IReferencable

		IReferenceCounter IReferencable.ReferenceCounter { get; set; }

		protected IReferenceCounter ReferenceCounter => ( (IReferencable)this ).ReferenceCounter;

		void IReferencable.OnAllocate()
		{
			OnAllocate();
		}

		void IReferencable.OnReset()
		{
			OnReset();
		}

		#endregion
	}

	public abstract class MessagePackReferencable : Referencable
	{
		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendFormat( "{0} (", GetType().Name );

			var fields = GetType().GetFields( BindingFlags.Public | BindingFlags.Instance );
			foreach( var f in fields )
			{
				var attr = f.GetCustomAttribute( typeof( KeyAttribute ) );
				if( attr == null )
					continue;

				builder.AppendFormat( "{0}={1}, ", f.Name, f.GetValue( this ) );
			}

			builder.Append( ")" );

			return builder.ToString();
		}

		protected override void OnReset()
		{
			base.OnReset();

			ReferenceCounter.Allocator.Simulator.Context.LogWarning( "Reflection OnReset {0}", this );
			var fields = GetType().GetFields( BindingFlags.Public | BindingFlags.Instance );
			foreach( var f in fields )
			{
				var attr = f.GetCustomAttribute( typeof( KeyAttribute ) );
				if( attr == null )
					continue;

				var defaultValue = f.FieldType.IsValueType ? Activator.CreateInstance( f.FieldType ) : null;
				f.SetValue( this, defaultValue );
			}
		}
	}

	public static class ReferencableExtensions
	{
		public static T Allocate<T>(this IReferencable referencable)
			 where T : class, IReferencable, new()
		{
			return referencable.ReferenceCounter.Allocate<T>();
		}

		public static IReferencable Allocate(this IReferencable referencable, IReferencable value)
		{
			return referencable.ReferenceCounter.Allocate( value );
		}

		public static IReferencable Allocate(this IReferencable referencable, Type type)
		{
			return referencable.ReferenceCounter.Allocate( type );
		}

		public static void Retain(this IReferencable referencable)
		{
			referencable.ReferenceCounter.Retain();
		}

		public static void Release(this IReferencable referencable)
		{
			referencable.ReferenceCounter.Release();
		}
	}

	public interface IReferenceCounter
	{
		void Retain();

		void Release();

		T Allocate<T>() where T : class, IReferencable, new();

		IReferencable Allocate(IReferencable value);

		IReferencable Allocate(Type type);

		int ReferencedCount { get; }

		IReferencable Value { get; }

		ReferencableAllocator Allocator { get; }
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

		public IReferencable Allocate(Type type)
		{
			return EnsurePool( type ).Allocate( type ).Value;
		}

		public IReferencable Allocate(IReferencable value)
		{
			return EnsurePool( value.GetType() ).Allocate( value ).Value;
		}

		public void Clear()
		{
			m_pools.Clear();
		}

		ReferencableCounterPool EnsurePool(Type type)
		{
			if( !m_pools.TryGetValue( type, out ReferencableCounterPool pool ) )
			{
				pool = new ReferencableCounterPool( this, type );
				m_pools.Add( type, pool );
			}

			return pool;
		}

		#region ReferencableCounterPool

		class ReferencableCounterPool
		{
			readonly List<IReferenceCounter> m_counters;
			readonly Queue<int> m_unreferenced;

			public ReferencableCounterPool(ReferencableAllocator allocator, Type type, int initialCapacity = 16)
			{
				Allocator = allocator;
				ReferencableType = type;
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
				return Allocate( (IReferencable)value );
			}

			public IReferenceCounter Allocate(Type type)
			{
				if( type != ReferencableType )
					throw new ArgumentException( nameof( type ) );

				if( m_unreferenced.Count > 0 )
				{
					var index = m_unreferenced.Dequeue();
					var counter = m_counters[index];
					counter.Retain();
					return counter;
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

			public IReferenceCounter Allocate(IReferencable value)
			{
				if( value.GetType() != ReferencableType )
					throw new ArgumentException( nameof( value ) );

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

			public void Release(int index)
			{
				m_unreferenced.Enqueue( index );
			}

			public ReferencableAllocator Allocator { get; private set; }

			public Type ReferencableType { get; private set; }
		}

		#endregion

		#region ReferencableCounter

		class ReferencableCounter : IReferenceCounter
		{
			readonly ReferencableCounterPool m_pool;
			readonly int m_index;
			readonly IReferencable m_value;
			int m_referencedCount;
#if ENABLE_ALLOCATOR_LOG
			List<string> m_logs;
#endif

			public ReferencableCounter(ReferencableCounterPool pool, int index, IReferencable value)
			{
				if( value.ReferenceCounter != null )
				{
#if ENABLE_ALLOCATOR_LOG
					( (ReferencableCounter)value.ReferenceCounter ).DumpLogs();
#endif
					throw new InvalidOperationException( $"{this} is already allocated" );
				}

#if ENABLE_ALLOCATOR_LOG
				if( pool.ReferencableType == typeof( Fps.PlayerConnectedEvent ) )
					m_logs = new List<string>();
#endif

				m_pool = pool;
				m_index = index;
				m_value = value;
				m_value.ReferenceCounter = this;
				m_value.OnAllocate();

				Retain();
			}

			public override string ToString()
			{
				return $"{m_pool.ReferencableType.Name}-{m_index}";
			}

			public void Retain()
			{
#if ENABLE_ALLOCATOR_LOG
				AppendLog( nameof( Retain ) );
#endif
				m_referencedCount++;
			}

			public void Release()
			{
#if ENABLE_ALLOCATOR_LOG
				AppendLog( nameof( Release ) );
#endif

				if( m_referencedCount == 0 )
				{
#if ENABLE_ALLOCATOR_LOG
					DumpLogs();
#endif
					throw new InvalidOperationException( $"{this} is already released" );
				}

				if( --m_referencedCount == 0 )
				{
					m_value.OnReset();

					if( m_index >= 0 )
						m_pool.Release( m_index );

#if ENABLE_ALLOCATOR_LOG
					//ClearLogs();
#endif
				}
			}

#if ENABLE_ALLOCATOR_LOG
			void AppendLog(string tag)
			{
				if( m_logs != null )
				{
					var log = string.Format( "{0}|{1}|{2}|{3}|{4}", m_pool.Allocator.Simulator.FixedTime, m_value, tag, m_referencedCount, Environment.StackTrace );

					m_logs.Add( log );
				}
			}

			void DumpLogs()
			{
				if( m_logs != null )
				{
					foreach( var log in m_logs )
						m_pool.Allocator.Simulator.Context.LogError( log );
				}
			}

			void ClearLogs()
			{
				if( m_logs != null )
					m_logs.Clear();
			}
#endif

			public T1 Allocate<T1>()
				where T1 : class, IReferencable, new()
			{
				return m_pool.Allocator.Allocate<T1>();
			}

			public IReferencable Allocate(IReferencable value)
			{
				return m_pool.Allocator.Allocate( value );
			}

			public IReferencable Allocate(Type type)
			{
				return m_pool.Allocator.Allocate( type );
			}

			public IReferencable Value => m_value;

			public int ReferencedCount => m_referencedCount;

			public ReferencableAllocator Allocator => m_pool.Allocator;
		}

		#endregion
	}
}
