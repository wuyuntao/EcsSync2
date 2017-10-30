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

		TickScheduler.TickContext m_context;
		Snapshot m_state;
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

			entity.SceneManager.Simulator.TickScheduler.AddComponent( this );

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

		internal void RecoverSnapshot(Snapshot state)
		{
			ValidateTickContext();

			if( state != null )
			{
				var timeline = EnsureTimeline();
				timeline.AddPoint( m_context.Time, state );
			}

			OnSnapshotRecovered( state );
		}

		internal void ReceiveCommand(Command command)
		{
			ValidateTickContext();

			OnCommandReceived( command );
		}

		protected internal void ApplyEvent(Event @event)
		{
			ValidateTickContext();

			State = OnEventApplied( @event );
			Entity.SceneManager.Simulator.EventBus.EnqueueEvent( @event );

			@event.Release();
		}

		#endregion

		#region Context Switching

		internal void EnterContext(TickScheduler.TickContext context)
		{
			Debug.Assert( m_context == null );
			Debug.Assert( m_state == null );

			m_context = context;
			m_state = GetState( m_context );
			m_stateChanged = false;
		}

		internal void LeaveContext()
		{
			if( m_context == null )
				throw new InvalidOperationException( "Has not enter any context" );

			if( m_stateChanged )
				SetState( m_context, m_state );

			m_context = null;
			m_state = null;
			m_stateChanged = false;
		}

		internal Snapshot GetState(TickScheduler.TickContext context)
		{
			ValidateTickContext();

			var timeline = EnsureTimeline();
			return timeline.GetPoint( context.Time );
		}

		internal void SetState(TickScheduler.TickContext context, Snapshot state)
		{
			ValidateTickContext();

			var timeline = EnsureTimeline();
			timeline.AddPoint( context.Time, state );
		}

		#endregion

		#region Public Interface

		protected abstract void OnInitialize();

		protected abstract void OnStart();

		protected abstract void OnDestroy();

		protected abstract Snapshot CreateSnapshot();

		protected abstract void OnSnapshotRecovered(Snapshot state);

		protected abstract void OnFixedUpdate();

		protected abstract void OnCommandReceived(Command command);

		// 完全确定性的修改，不依赖于其他 Scene 或 Component 的状态
		protected abstract Snapshot OnEventApplied(Event @event);

		protected internal Snapshot State
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

		#endregion

		#region Internal Helpers

		void ValidateTickContext()
		{
			if( m_context == null )
				throw new InvalidOperationException( "Tick context not exist" );
		}

		Timeline EnsureTimeline()
		{
			Timeline timeline;
			switch( m_context.Type )
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
					throw new NotSupportedException( m_context.Type.ToString() );
			}

			return timeline;
		}

		Timeline EnsureTimeline(ref Timeline timeline)
		{
			timeline = timeline ?? new Timeline( null, Configuration.TimelineDefaultCapacity );
			return timeline;
		}

		Snapshot GetState()
		{
			switch( m_context.Type )
			{
				case TickScheduler.TickContextType.Sync:
					var timeline = EnsureTimeline();
					return timeline.InterpolatePoint( m_context.Time );

				default:
					throw new NotSupportedException( m_context.Type.ToString() );
			}
		}

		#endregion
	}
}
