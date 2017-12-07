using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EcsSync2
{
	public class ComponentSettings
	{
	}

	public abstract class Component : Disposable
	{
		public EventHandler<Component> OnStateReconciled;

		public TickScheduler TickScheduler { get; private set; }
		public Entity Entity { get; private set; }
		public InstanceId Id { get; private set; }
		public ComponentSettings Settings { get; private set; }

		TickScheduler.TickContext? m_context;
		ComponentSnapshot m_state;

		Timeline m_syncTimeline;
		Timeline m_reconciliationTimeline;
		Timeline m_predictionTimeline;
		List<IDisposable> m_eventHandlers;

		public TickScheduler.TickContextType? TickType => m_context != null ? (TickScheduler.TickContextType?)m_context.Value.Type : null;

		#region Life-cycle

		internal void Initialize(Entity entity, InstanceId id, ComponentSettings settings)
		{
			Entity = entity;
			Id = id;
			Settings = settings;

			TickScheduler = entity.SceneManager.Simulator.TickScheduler;
			TickScheduler.AddComponent( this );

			OnStateReconciled = CreateEventHandler<Component>();

			OnInitialize();
		}

		public override string ToString()
		{
			return $"{GetType().Name}-{Id}";
		}

		internal void Start()
		{
			EnsureTickContext( false );

			var state = CreateSnapshot();
			if( state != null )
			{
				State = state;
				state.Release();
			}

			OnStart();
		}

		internal void Destroy()
		{
			EnsureTickContext();

			OnDestroy();
		}

		protected override void DisposeManaged()
		{
			m_state?.Release();
			m_state = null;
			m_syncTimeline?.Clear();
			m_reconciliationTimeline?.Clear();
			m_predictionTimeline?.Clear();
			SafeDispose( m_eventHandlers );
			m_eventHandlers = null;

			base.DisposeManaged();
		}

		internal void FixedUpdate()
		{
			EnsureTickContext();

			OnFixedUpdate();
		}

		internal void RecoverSnapshot(ComponentSnapshot state, bool isReconciliation = false)
		{
			Debug.Assert( state != null );

			EnsureTickContext();

			//Entity.SceneManager.Simulator.Context.Log( "RecoverSnapshot {0}, {1}, {2}", this, m_context, state );

			switch( m_context.Value.Type )
			{
				case TickScheduler.TickContextType.Reconciliation:
					{
						var timeline = EnsureTimeline( TickScheduler.TickContextType.Reconciliation, ref m_reconciliationTimeline );
						timeline.Clear();
						break;
					}

				case TickScheduler.TickContextType.Prediction:
					{
						var timeline = EnsureTimeline( TickScheduler.TickContextType.Prediction, ref m_predictionTimeline );
						timeline.Clear();
						break;
					}
			}

			SetState( m_context.Value, state );

			OnSnapshotRecovered( state );

			if( isReconciliation )
				OnStateReconciled.Invoke( this );
		}

		internal void ReceiveCommand(ComponentCommand command)
		{
			EnsureTickContext();

			//Entity.SceneManager.Simulator.Context.Log( "ReceiveCommand {0}, {1}, {2}", this, m_context, command );

			OnCommandReceived( command );
		}

		protected internal void ApplyEvent(ComponentEvent @event)
		{
			EnsureTickContext();

			//Entity.SceneManager.Simulator.Context.Log( "ApplyEvent {0}, {1}, {2}", this, m_context, @event );

			var state = OnEventApplied( @event );
			State = state;
			state.Release();

			if( Entity.SceneManager.Simulator.ServerTickScheduler != null )
				Entity.SceneManager.Simulator.ServerTickScheduler.EnqueueEvent( @event );

			@event.Release();
		}

		internal void Interpolate()
		{
			EnsureTickContext();

			// 触发缓存的事件
			if( Entity.IsLocalEntity )
			{

			}
			else
			{

			}
		}

		#endregion

		#region Public Interface

		protected abstract void OnInitialize();

		protected abstract void OnStart();

		protected abstract void OnDestroy();

		protected internal abstract ComponentSnapshot CreateSnapshot();

		protected abstract void OnSnapshotRecovered(ComponentSnapshot state);

		protected abstract void OnFixedUpdate();

		protected abstract void OnCommandReceived(ComponentCommand command);

		// 完全确定性的修改，不依赖于其他 Scene 或 Component 的状态
		protected abstract ComponentSnapshot OnEventApplied(ComponentEvent @event);

		protected internal ComponentSnapshot State
		{
			get
			{
				EnsureTickContext();

				return m_state;
			}
			internal set
			{
				EnsureTickContext();

				if( ReferenceEquals( m_state, value ) )
					throw new InvalidOperationException( $"State {m_state} is set multiple times" );

				m_state?.Release();
				m_state = value;
				m_state.Retain();
				SetState( m_context.Value, value );
			}
		}

		protected TSnapshot CreateSnapshot<TSnapshot>()
			where TSnapshot : ComponentSnapshot, new()
		{
			EnsureTickContext();

			var s = Entity.SceneManager.Simulator.ReferencableAllocator.Allocate<TSnapshot>();
			s.ComponentId = Id;
			return s;
		}

		protected TEvent CreateEvent<TEvent>()
			where TEvent : ComponentEvent, new()
		{
			EnsureTickContext();

			var e = Entity.SceneManager.Simulator.ReferencableAllocator.Allocate<TEvent>();
			e.ComponentId = Id;
			return e;
		}

		protected EventHandler CreateEventHandler()
		{
			return CreateEventHandler( new EventHandler( Entity.SceneManager.Simulator.EventDispatcher, EventHandler_OnInvoke ) );
		}

		protected EventHandler<T1> CreateEventHandler<T1>()
		{
			return CreateEventHandler( new EventHandler<T1>( Entity.SceneManager.Simulator.EventDispatcher, EventHandler_OnInvoke ) );
		}

		protected EventHandler<T1, T2> CreateEventHandler<T1, T2>()
		{
			return CreateEventHandler( new EventHandler<T1, T2>( Entity.SceneManager.Simulator.EventDispatcher, EventHandler_OnInvoke ) );
		}

		protected EventHandler<T1, T2, T3> CreateEventHandler<T1, T2, T3>()
		{
			return CreateEventHandler( new EventHandler<T1, T2, T3>( Entity.SceneManager.Simulator.EventDispatcher, EventHandler_OnInvoke ) );
		}

		protected EventHandler<T1, T2, T3, T4> CreateEventHandler<T1, T2, T3, T4>()
		{
			return CreateEventHandler( new EventHandler<T1, T2, T3, T4>( Entity.SceneManager.Simulator.EventDispatcher, EventHandler_OnInvoke ) );
		}

		void EventHandler_OnInvoke(EventDispatcher.EventInvocation invocation)
		{
			EnsureTickContext();

			//AddEventInvocation( m_context, invocation );
		}

		#endregion

		#region Private Helpers

		void EnsureTickContext(bool getState = true)
		{
			if( TickScheduler.CurrentContext == null )
				throw new InvalidOperationException( "Tick context not exist" );

			if( m_context == TickScheduler.CurrentContext.Value )
				return;

			m_context = TickScheduler.CurrentContext.Value;

			if( getState )
			{
				m_state = GetState( m_context.Value );
				m_state.Retain();
			}
		}

		internal ComponentSnapshot GetState(TickScheduler.TickContext context)
		{
			switch( context.Type )
			{
				case TickScheduler.TickContextType.Sync:
					return GetSyncState( context );

				case TickScheduler.TickContextType.Reconciliation:
					if( m_reconciliationTimeline != null && m_reconciliationTimeline.TryFind( context.Time, out ComponentSnapshot s1 ) )
						return s1;

					return GetSyncState( context );

				case TickScheduler.TickContextType.Prediction:
					if( m_predictionTimeline != null && m_predictionTimeline.TryFind( context.Time, out ComponentSnapshot s2 ) )
						return s2;

					return GetSyncState( context );

				case TickScheduler.TickContextType.Interpolation:
					if( m_predictionTimeline != null && m_predictionTimeline.TryInterpolate( context.Time, out ComponentSnapshot s3 ) )
						return s3;

					return InterpolateSyncState( context, out s3 );

				default:
					throw new NotSupportedException( context.Type.ToString() );
			}
		}

		ComponentSnapshot GetSyncState(TickScheduler.TickContext context)
		{
			// 如果是纯客户端本地预测对象，可能没有 sync timeline
			if( m_syncTimeline != null )
			{
				if( m_syncTimeline.TryFind( context.Time, out ComponentSnapshot snapshot ) )
					return snapshot;

				if( m_syncTimeline.FirstPoint != null )
				{
					Entity.SceneManager.Simulator.Context.LogWarning( "Cannot find sync snapshot for {0}, Use first snapshot instead {1}", context, m_syncTimeline.FirstPoint );

					return m_syncTimeline.FirstPoint.Snapshot;
				}
			}

			throw new InvalidOperationException( $"Cannot find sync snapshot of {this} for {context}" );
		}

		ComponentSnapshot InterpolateSyncState(TickScheduler.TickContext context, out ComponentSnapshot s3)
		{
			if( m_syncTimeline != null )
			{
				if( m_syncTimeline.TryInterpolate( context.Time, out s3 ) )
					return s3;

				if( m_syncTimeline.FirstPoint != null )
				{
					Entity.SceneManager.Simulator.Context.LogWarning( "Cannot find sync snapshot for {0}, Use first snapshot instead {1}", context, m_syncTimeline.FirstPoint );

					return m_syncTimeline.FirstPoint.Snapshot;
				}
			}

			throw new InvalidOperationException( $"Cannot find sync snapshot of {this} for {context}" );
		}

		void SetState(TickScheduler.TickContext context, ComponentSnapshot state)
		{
			switch( context.Type )
			{
				case TickScheduler.TickContextType.Sync:
					EnsureTimeline( context.Type, ref m_syncTimeline ).Add( context.Time, state );
					break;

				case TickScheduler.TickContextType.Reconciliation:
					EnsureTimeline( context.Type, ref m_reconciliationTimeline ).Add( context.Time, state );
					break;

				case TickScheduler.TickContextType.Prediction:
					EnsureTimeline( context.Type, ref m_predictionTimeline ).Add( context.Time, state );

					if( TickScheduler is ClientTickScheduler cts )
						cts.AddPredictiveComponents( this );
					break;

				case TickScheduler.TickContextType.Interpolation:
					// No need to save interpolated states
					break;

				default:
					throw new NotSupportedException( context.Type.ToString() );
			}
		}

		//void AddEventInvocation(TickScheduler.TickContext context, EventDispatcher.EventInvocation invocation)
		//{
		//	switch( context.Type )
		//	{
		//		case TickScheduler.TickContextType.Sync:
		//			EnsureTimeline( context.Type, ref m_syncTimeline ).Add( context.Time, invocation );
		//			break;

		//		case TickScheduler.TickContextType.Reconciliation:
		//			EnsureTimeline( context.Type, ref m_reconciliationTimeline ).Add( context.Time, invocation );
		//			break;

		//		case TickScheduler.TickContextType.Prediction:
		//			EnsureTimeline( context.Type, ref m_predictionTimeline ).Add( context.Time, invocation );
		//			break;

		//		case TickScheduler.TickContextType.Interpolation:
		//			// No need to save interpolated states
		//			break;

		//		default:
		//			throw new NotSupportedException( context.Type.ToString() );
		//	}
		//}

		Timeline EnsureTimeline(TickScheduler.TickContextType type, ref Timeline timeline)
		{
			timeline = timeline ?? new Timeline( Entity.SceneManager.Simulator.ReferencableAllocator, type );
			return timeline;
		}

		internal void RemoveStatesBefore(TickScheduler.TickContext context)
		{
			switch( context.Type )
			{
				case TickScheduler.TickContextType.Sync:
					var removed1 = m_syncTimeline?.RemoveBefore( context.Time );
					//Entity.SceneManager.Simulator.Context.Log( "{0}: RemoveStatesBefore {1}, removed {2}", this, context, removed1 );
					break;

				case TickScheduler.TickContextType.Prediction:
					var removed2 = m_predictionTimeline?.RemoveBefore( context.Time );
					//Entity.SceneManager.Simulator.Context.Log( "{0}: RemoveStatesBefore {1}, removed {2}", this, context, removed2 );
					break;

				default:
					throw new NotSupportedException( context.Type.ToString() );
			}
		}

		T CreateEventHandler<T>(T handler)
			where T : IDisposable
		{
			if( m_eventHandlers == null )
				m_eventHandlers = new List<IDisposable>();

			m_eventHandlers.Add( handler );
			return handler;
		}

		#endregion
	}
}
