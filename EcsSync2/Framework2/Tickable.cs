using System;

namespace EcsSync2.Framework2
{
	abstract class Tickable
	{
		TickableScheduler m_scheduler;
		TickableScheduler.ITickContext m_context;
		Snapshot m_state;

		Timeline m_syncTimeline;
		Timeline m_reconcilationTimeline;
		Timeline m_predictionTimeline;
		Timeline m_interpolationTimeline;

		protected Tickable(TickableScheduler scheduler)
		{
			m_scheduler = scheduler;
		}

		internal void Start()
		{
			EnsureTickContext();

			var state = OnStart();
			var timeline = EnsureTimeline();
			timeline.AddPoint( m_context.Time, state );
		}

		protected abstract Snapshot OnStart();

		protected abstract void OnFixedUpdate();

		// 完全确定性的修改，不依赖于其他 Scene 或 Component 的状态
		protected abstract Snapshot OnEventApplied(Snapshot state, Event @event);

		void EnsureTickContext()
		{
			// 确保 TickContext 和缓存的 State 一致
			m_context = m_scheduler.CurrentContext;
		}

		Timeline EnsureTimeline()
		{
			switch( m_context.Mode )
			{
				case TickableScheduler.TickMode.Sync:
					return EnsureTimeline( ref m_syncTimeline );

				case TickableScheduler.TickMode.Reconcilation:
					return EnsureTimeline( ref m_reconcilationTimeline );

				case TickableScheduler.TickMode.Prediction:
					return EnsureTimeline( ref m_predictionTimeline );

				case TickableScheduler.TickMode.Interpolation:
					return EnsureTimeline( ref m_interpolationTimeline );

				default:
					throw new NotSupportedException( m_context.Mode.ToString() );
			}
		}

		Timeline EnsureTimeline(ref Timeline timeline)
		{
			timeline = timeline ?? new Timeline( null, Settings.TimelineDefaultCapacity );
			return timeline;
		}

		protected Snapshot State => m_state;
	}
}
