using System;
using System.Diagnostics;

namespace EcsSync2
{
	public class ComponentSettings
	{
	}

	public abstract class Component
	{
		public Entity Entity { get; private set; }
		public InstanceId Id { get; private set; }
		public ComponentSettings Settings { get; private set; }

		TickScheduler m_tickScheduler;
		ComponentSnapshot m_state;
		bool m_stateChanged;

		Timeline m_syncTimeline;
		Timeline m_reconcilationTimeline;
		Timeline m_predictionTimeline;
		Timeline m_interpolationTimeline;

		#region Life-cycle

		internal void Initialize(Entity entity, InstanceId id, ComponentSettings settings)
		{
			Entity = entity;
			Id = id;
			Settings = settings;

			m_tickScheduler = entity.SceneManager.Simulator.TickScheduler;
			m_tickScheduler.AddComponent( this );

			OnInitialize();
		}

		internal void Start()
		{
			ValidateTickContext();

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
			ValidateTickContext();

			OnDestroy();
		}

		internal void FixedUpdate()
		{
			ValidateTickContext();

			OnFixedUpdate();
		}

		internal void RecoverSnapshot(ComponentSnapshot state)
		{
			ValidateTickContext();

			if( state != null )
			{
				var timeline = EnsureTimeline( m_tickScheduler.CurrentContext.Value );
				timeline.AddPoint( m_tickScheduler.CurrentContext.Value.Time, state );
			}

			OnSnapshotRecovered( state );
		}

		internal void ReceiveCommand(ComponentCommand command)
		{
			ValidateTickContext();

			OnCommandReceived( command );
		}

		protected internal void ApplyEvent(ComponentEvent @event)
		{
			ValidateTickContext();

			State = OnEventApplied( @event );
			Entity.SceneManager.Simulator.EventBus.EnqueueEvent( m_tickScheduler.CurrentContext.Value.Time, @event );

			@event.Release();
		}

		#endregion

		#region Context Switching

		internal void EnterContext(TickScheduler.TickContext context)
		{
			Debug.Assert( m_state == null );

			m_state = GetState( m_tickScheduler.CurrentContext.Value );
			m_stateChanged = false;
		}

		internal void LeaveContext()
		{
			ValidateTickContext();

			if( m_stateChanged )
				SetState( m_tickScheduler.CurrentContext.Value, m_state );

			m_state = null;
			m_stateChanged = false;
		}

		internal ComponentSnapshot GetState(TickScheduler.TickContext context)
		{
			var timeline = EnsureTimeline( context );
			return (ComponentSnapshot)timeline.GetPoint( context.Time );
		}

		internal void SetState(TickScheduler.TickContext context, ComponentSnapshot state)
		{
			var timeline = EnsureTimeline( context );
			timeline.AddPoint( context.Time, state );
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
				ValidateTickContext();

				return m_state;
			}
			internal set
			{
				ValidateTickContext();

				if( m_state == value )
					return;

				m_state?.Release();
				m_state = value;
				m_state.Retain();
				m_stateChanged = true;
			}
		}

		protected TSnapshot AllocateSnapshot<TSnapshot>()
			where TSnapshot : ComponentSnapshot, new()
		{
			ValidateTickContext();

			var s = Entity.SceneManager.Simulator.ReferencableAllocator.Allocate<TSnapshot>();
			s.ComponentId = Id;
			return s;
		}

		protected TEvent AllocateEvent<TEvent>()
			where TEvent : ComponentEvent, new()
		{
			ValidateTickContext();

			var e = Entity.SceneManager.Simulator.ReferencableAllocator.Allocate<TEvent>();
			e.ComponentId = Id;
			return e;
		}

		#endregion

		#region Internal Helpers

		void ValidateTickContext()
		{
			if( m_tickScheduler.CurrentContext == null )
				throw new InvalidOperationException( "Tick context not exist" );
		}

		Timeline EnsureTimeline(TickScheduler.TickContext context)
		{
			Timeline timeline;
			switch( context.Type )
			{
				case TickScheduler.TickContextType.Sync:
					timeline = EnsureTimeline( ref m_syncTimeline );
					break;

				case TickScheduler.TickContextType.Reconcilation:
					timeline = EnsureTimeline( ref m_reconcilationTimeline );
					if( timeline.Count == 0 )
						timeline = EnsureTimeline( ref m_syncTimeline );
					break;

				case TickScheduler.TickContextType.Prediction:
					timeline = EnsureTimeline( ref m_predictionTimeline );
					if( timeline.Count == 0 )
						timeline = EnsureTimeline( ref m_syncTimeline );
					break;

				case TickScheduler.TickContextType.Interpolation:
					timeline = EnsureTimeline( ref m_interpolationTimeline );
					break;

				default:
					throw new NotSupportedException( context.Type.ToString() );
			}

			return timeline;
		}

		Timeline EnsureTimeline(ref Timeline timeline)
		{
			timeline = timeline ?? new Timeline( null, Configuration.TimelineDefaultCapacity );
			return timeline;
		}

		#endregion
	}
}
