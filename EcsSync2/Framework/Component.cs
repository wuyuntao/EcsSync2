﻿using System;
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

		internal void Start()
		{
			EnsureTickContext();

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

			State = OnEventApplied( @event );
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

				if( m_state == value )
					return;

				m_state?.Release();
				m_state = value;
				m_state.Retain();
				SetState( m_context.Value, value );
			}
		}

		protected TSnapshot AllocateSnapshot<TSnapshot>()
			where TSnapshot : ComponentSnapshot, new()
		{
			EnsureTickContext();

			var s = Entity.SceneManager.Simulator.ReferencableAllocator.Allocate<TSnapshot>();
			s.ComponentId = Id;
			return s;
		}

		protected TEvent AllocateEvent<TEvent>()
			where TEvent : ComponentEvent, new()
		{
			EnsureTickContext();

			var e = Entity.SceneManager.Simulator.ReferencableAllocator.Allocate<TEvent>();
			e.ComponentId = Id;
			return e;
		}

		#endregion

		#region Private Helpers

		void EnsureTickContext()
		{
			if( TickScheduler.CurrentContext == null )
				throw new InvalidOperationException( "Tick context not exist" );

			if( m_context == TickScheduler.CurrentContext.Value )
				return;

			m_context = TickScheduler.CurrentContext.Value;
			m_state = GetState( m_context.Value );
		}

		internal ComponentSnapshot GetState(TickScheduler.TickContext context)
		{
			switch( context.Type )
			{
				case TickScheduler.TickContextType.Sync:
					{
						Debug.Assert( m_syncTimeline != null );

						if( m_syncTimeline.TryFind( context.Time, out ComponentSnapshot snapshot ) )
							return snapshot;

						throw new InvalidOperationException( $"Cannot find snapshot for {context}" );
					}

				case TickScheduler.TickContextType.Reconcilation:
					{
						Debug.Assert( m_syncTimeline != null );

						if( m_reconcilationTimeline != null && m_reconcilationTimeline.TryFind( context.Time, out ComponentSnapshot snapshot ) )
							return snapshot;

						if( m_syncTimeline.TryFind( context.Time, out snapshot ) )
							return snapshot;

						throw new InvalidOperationException( $"Cannot find snapshot for {context}" );
					}

				case TickScheduler.TickContextType.Prediction:
				case TickScheduler.TickContextType.Interpolation:
					{
						Debug.Assert( m_syncTimeline != null );

						if( m_predictionTimeline != null && m_predictionTimeline.TryFind( context.Time, out ComponentSnapshot snapshot ) )
							return snapshot;

						if( m_syncTimeline.TryFind( context.Time, out snapshot ) )
							return snapshot;

						throw new InvalidOperationException( $"Cannot find snapshot for {context}" );
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
					EnsureTimeline( ref m_syncTimeline ).Add( context.Time, state );
					break;

				case TickScheduler.TickContextType.Reconcilation:
					EnsureTimeline( ref m_reconcilationTimeline ).Add( context.Time, state );
					break;

				case TickScheduler.TickContextType.Prediction:
					EnsureTimeline( ref m_predictionTimeline ).Add( context.Time, state );
					break;

				case TickScheduler.TickContextType.Interpolation:
					// No need to save interpolated states
					break;

				default:
					throw new NotSupportedException( context.Type.ToString() );
			}
		}

		Timeline EnsureTimeline(ref Timeline timeline)
		{
			timeline = timeline ?? new Timeline( Entity.SceneManager.Simulator.ReferencableAllocator );
			return timeline;
		}

		#endregion
	}
}
