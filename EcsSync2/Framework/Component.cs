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
		}

		internal void Start()
		{
			ValidateTickContext();

			var state = OnStart();
			var timeline = EnsureTimeline();
			timeline.AddPoint( m_context.Time, state );
		}


		internal void FixedUpdate()
		{
			ValidateTickContext();

			OnFixedUpdate();
		}

		internal void RecoverSnapshot(Snapshot cs)
		{
			ValidateTickContext();
		}

		internal void ReceiveCommand(Command command)
		{
		}

		protected internal void ApplyEvent(Event @event)
		{
			ValidateTickContext();
		}

		#endregion

		#region Context Switching

		internal void EnterContext(TickScheduler.TickContext context)
		{
			Debug.Assert( m_context == null );
			Debug.Assert( m_state == null );

			m_context = context;
			// TODO GetState
		}

		internal void LeaveContext()
		{
			if( m_context == null )
				throw new InvalidOperationException( "Has not enter any context" );

			// TODO SetState
			m_context = null;
		}

		internal Snapshot GetState(TickScheduler.TickContext context)
		{
			ValidateTickContext();

			throw new NotSupportedException();
		}
		internal void SetState(TickScheduler.TickContext context, Snapshot snapshot)
		{
			ValidateTickContext();

			throw new NotSupportedException();
		}

		#endregion

		#region Public Interface

		protected abstract Snapshot OnStart();

		protected abstract void OnFixedUpdate();

		protected abstract void OnSnapshotRecovered(Snapshot state);

		protected abstract void OnCommandReceived(Command command);

		// 完全确定性的修改，不依赖于其他 Scene 或 Component 的状态
		protected abstract Snapshot OnEventApplied(Event @event);

		protected Snapshot State
		{
			get
			{
				ValidateTickContext();
				return m_state;
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
			switch( m_context.Type )
			{
				case TickScheduler.TickContextType.Sync:
					return EnsureTimeline( ref m_syncTimeline );

				case TickScheduler.TickContextType.Reconcilation:
					return EnsureTimeline( ref m_reconcilationTimeline );

				case TickScheduler.TickContextType.Prediction:
					return EnsureTimeline( ref m_predictionTimeline );

				case TickScheduler.TickContextType.Interpolation:
					return EnsureTimeline( ref m_interpolationTimeline );

				default:
					throw new NotSupportedException( m_context.Type.ToString() );
			}
		}

		Timeline EnsureTimeline(ref Timeline timeline)
		{
			timeline = timeline ?? new Timeline( null, EcsSync2.Configuration.TimelineDefaultCapacity );
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

		void SetState()
		{
			m_state.Release();
			m_state = null;
		}

		#endregion
	}
}
