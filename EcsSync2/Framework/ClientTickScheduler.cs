using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public class ClientTickScheduler : TickScheduler
	{
		TickContext m_syncTickContext = new TickContext( TickContextType.Sync );
		TickContext m_reconcilationTickContext = new TickContext( TickContextType.Reconcilation );
		TickContext m_predictionTickContext = new TickContext( TickContextType.Prediction );
		Queue<SyncFrame> m_syncFrames = new Queue<SyncFrame>();

		public ClientTickScheduler(Simulator simulator)
			: base( simulator )
		{
		}

		internal override void Tick()
		{
			ApplySyncFrames();
			ReconcilePredictions();
			Predict();
		}

		#region Synchronization

		public void ReceiveSyncFrame(SyncFrame frame)
		{
			m_syncFrames.Enqueue( frame );
		}

		void ApplySyncFrames()
		{
			while( m_syncFrames.Count > 0 )
			{
				var frame = m_syncFrames.Dequeue();

				m_syncTickContext.Time = frame.Time;
				EnterContext( m_syncTickContext );

				if( frame is FullSyncFrame fsf )
					ApplyFullSyncFrame( fsf );
				else if( frame is DeltaSyncFrame dsf )
					ApplyDeltaSyncFrame( dsf );
				else
					throw new NotSupportedException();

				LeaveContext();
			}
		}

		void ApplyFullSyncFrame(FullSyncFrame frame)
		{
			foreach( var es in frame.Entities )
			{
				Simulator.SceneManager.Scene.CreateEntity( es.Id, es.Settings );

				foreach( var cs in es.Components )
				{
					var component = Simulator.SceneManager.FindComponent( cs.Id );
					component.RecoverSnapshot( cs );
				}
			}
		}

		void ApplyDeltaSyncFrame(DeltaSyncFrame frame)
		{
			foreach( var e in frame.Events )
			{
				if( e is SceneEvent se )
				{
					Simulator.SceneManager.Scene.ApplyEvent( se );
				}
				else if( e is ComponentEvent ce )
				{
					var component = Simulator.SceneManager.FindComponent( ce.ComponentId );
					component.ApplyEvent( ce );
				}
				else
					throw new NotSupportedException( e.ToString() );
			}
		}

		#endregion

		#region Reconcilation

		void ReconcilePredictions()
		{
			// 没有新的同步帧，或没有新的预测帧需要和解
			if( m_syncTickContext.Time <= m_reconcilationTickContext.Time ||
				m_predictionTickContext.Time < m_syncTickContext.Time )
				return;

			// 更新和解时间
			m_reconcilationTickContext.Time = m_syncTickContext.Time;

			var components = GetPredictedComponents();

			// 判断是否需要和解
			if( !RequireReconcilation( components ) )
				return;

			// 回滚到同步状态
			foreach( var component in components )
			{
				var syncState = component.GetState( m_syncTickContext );
				component.SetState( m_reconcilationTickContext, syncState );
			}

			// 以和解模式更新到最新预测的状态
			while( m_reconcilationTickContext.Time < m_predictionTickContext.Time )
			{
				m_reconcilationTickContext.Time += Configuration.SimulationDeltaTime;

				EnterContext( m_reconcilationTickContext );
				DispatchCommands( m_reconcilationTickContext );
				FixedUpdate();
				LeaveContext();
			}

			// 以和解后的状态和最新预测的状态的中间值，来纠正最新的预测
			foreach( var component in components )
			{
				var reconcilationState = component.GetState( m_reconcilationTickContext );
				if( Configuration.ComponentReconcilationRatio < 1 )
				{
					var predictionState = component.GetState( m_predictionTickContext );
					reconcilationState = predictionState.Interpolate(reconcilationState, Configuration.ComponentReconcilationRatio);
				}
				component.SetState( m_predictionTickContext, reconcilationState );
			}

			// 重置和解时间
			m_reconcilationTickContext.Time = m_syncTickContext.Time;

			// 清理已确认命令
			Simulator.CommandQueue.DequeueBefore( Simulator.LocalUserId.Value, m_syncTickContext.Time );
		}

		List<Component> GetPredictedComponents()
		{
			throw new NotImplementedException();
		}

		bool RequireReconcilation(List<Component> components)
		{
			foreach( var component in components )
			{
				var syncState = component.GetState( m_syncTickContext );
				var predictionState = component.GetState( m_reconcilationTickContext );
				if( !syncState.IsApproximate( predictionState ) )
					return true;
			}

			return false;
		}

		#endregion

		#region Prediction

		void Predict()
		{
			m_predictionTickContext.Time = Simulator.FixedTime;

			EnterContext( m_predictionTickContext );
			Simulator.InputManager?.SetInput();
			Simulator.InputManager?.EnqueueCommands();
			DispatchCommands( m_predictionTickContext );
			FixedUpdate();
			Simulator.InputManager.ResetInput();
			LeaveContext();
		}

		void DispatchCommands(TickContext ctx)
		{
			var frame = Simulator.CommandQueue.FetchCommands( Simulator.LocalUserId.Value, ctx.Time );

			DispatchCommands( frame );
		}

		#endregion

		public CommandFrame FetchCommandFrame()
		{
			throw new NotImplementedException();
		}
	}
}
