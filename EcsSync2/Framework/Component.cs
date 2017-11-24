using System;
using System.Diagnostics;

namespace EcsSync2
{
	public class ComponentSettings
	{
	}

	public abstract class Component : Disposable
	{
		public TickScheduler TickScheduler { get; private set; }
		public Entity Entity { get; private set; }
		public InstanceId Id { get; private set; }
		public ComponentSettings Settings { get; private set; }

		TickScheduler.TickContext? m_context;
		ComponentSnapshot m_state;

		Timeline m_syncTimeline;
		Timeline m_reconcilationTimeline;
		Timeline m_predictionTimeline;

		public TickScheduler.TickContextType? TickType => m_context != null ? (TickScheduler.TickContextType?)m_context.Value.Type : null;

		#region Life-cycle

		internal void Initialize(Entity entity, InstanceId id, ComponentSettings settings)
		{
			Entity = entity;
			Id = id;
			Settings = settings;

			TickScheduler = entity.SceneManager.Simulator.TickScheduler;
			TickScheduler.AddComponent( this );

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
			m_reconcilationTimeline?.Clear();
			m_predictionTimeline?.Clear();

			base.DisposeManaged();
		}

		internal void FixedUpdate()
		{
			EnsureTickContext();

			OnFixedUpdate();
		}

		internal void RecoverSnapshot(ComponentSnapshot state)
		{
			Debug.Assert( state != null );

			EnsureTickContext();

			switch( m_context.Value.Type )
			{
				case TickScheduler.TickContextType.Reconcilation:
					{
						var timeline = EnsureTimeline( TickScheduler.TickContextType.Reconcilation, ref m_reconcilationTimeline );
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
		}

		internal void ReceiveCommand(ComponentCommand command)
		{
			EnsureTickContext();

			OnCommandReceived( command );
		}

		protected internal void ApplyEvent(ComponentEvent @event)
		{
			EnsureTickContext();

			var state = OnEventApplied( @event );
			State = state;
			state.Release();

			if( Entity.SceneManager.Simulator.ServerTickScheduler != null )
				Entity.SceneManager.Simulator.EventBus.EnqueueEvent( TickScheduler.CurrentContext.Value.Time, @event );

			@event.Release();
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
					{
						if( m_syncTimeline == null )
							throw new InvalidOperationException( $"Missing sync timeline of {this}" );

						if( m_syncTimeline.TryFind( context.Time, out ComponentSnapshot snapshot ) )
							return snapshot;

						throw new InvalidOperationException( $"Cannot find snapshot of {this} for {context}" );
					}

				case TickScheduler.TickContextType.Reconcilation:
					{
						if( m_syncTimeline == null )
							throw new InvalidOperationException( $"Missing sync timeline of {this}" );

						if( m_reconcilationTimeline != null && m_reconcilationTimeline.TryFind( context.Time, out ComponentSnapshot snapshot ) )
							return snapshot;

						if( m_syncTimeline.TryFind( context.Time, out snapshot ) )
							return snapshot;

						throw new InvalidOperationException( $"Cannot find snapshot of {this} for {context}" );
					}

				case TickScheduler.TickContextType.Prediction:
				case TickScheduler.TickContextType.Interpolation:
					{
						if( m_syncTimeline == null )
							throw new InvalidOperationException( $"Missing sync timeline of {this}" );

						if( m_predictionTimeline != null && m_predictionTimeline.TryFind( context.Time, out ComponentSnapshot snapshot ) )
							return snapshot;

						if( m_syncTimeline.TryFind( context.Time, out snapshot ) )
							return snapshot;

						throw new InvalidOperationException( $"Cannot find snapshot of {this} for {context}" );
					}

				default:
					throw new NotSupportedException( context.Type.ToString() );
			}
		}

		void SetState(TickScheduler.TickContext context, ComponentSnapshot state)
		{
			switch( context.Type )
			{
				case TickScheduler.TickContextType.Sync:
					EnsureTimeline( context.Type, ref m_syncTimeline ).Add( context.Time, state );
					break;

				case TickScheduler.TickContextType.Reconcilation:
					EnsureTimeline( context.Type, ref m_reconcilationTimeline ).Add( context.Time, state );
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
					/*var removed1 = */m_syncTimeline?.RemoveBefore( context.Time );
					//Entity.SceneManager.Simulator.Context.Log( "{0}: RemoveStatesBefore {1}, removed {2}", this, context, removed1 );
					break;

				case TickScheduler.TickContextType.Prediction:
					/*var removed2 = */m_predictionTimeline?.RemoveBefore( context.Time );
					//Entity.SceneManager.Simulator.Context.Log( "{0}: RemoveStatesBefore {1}, removed {2}", this, context, removed2 );
					break;

				default:
					throw new NotSupportedException( context.Type.ToString() );
			}
		}

		#endregion
	}
}
