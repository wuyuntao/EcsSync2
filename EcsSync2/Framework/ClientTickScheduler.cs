using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EcsSync2
{
	public class ClientTickScheduler : TickScheduler
	{
		TickContext m_syncTickContext = new TickContext( TickContextType.Sync, 0 );
		TickContext m_reconciliationTickContext = new TickContext( TickContextType.Reconciliation, 0 );
		TickContext m_predictionTickContext = new TickContext( TickContextType.Prediction, 0 );
		TickContext m_interpolationTickContext = new TickContext( TickContextType.Interpolation, 0 );

		List<Component> m_syncedComponents = new List<Component>();
		List<Component> m_predictiveComponents = new List<Component>();
		Queue<SyncFrame> m_syncFrames = new Queue<SyncFrame>();
		Queue<CommandFrame> m_commandFrames = new Queue<CommandFrame>();

		public uint? StartFixedTime { get; private set; }

		public ClientTickScheduler(Simulator simulator)
			: base( simulator )
		{
		}

		internal override void Tick()
		{
			ApplySyncFrames();
			ReconcilePredictions();
			Predict();

			//Simulator.Context.Log( "Tick sync: {0}, predict: {1}", m_syncTickContext.Time, m_predictionTickContext.Time );
		}

		#region Synchronization

		public void ReceiveSyncFrame(SyncFrame frame)
		{
			Debug.Assert( frame.Time != 0 );

			if( StartFixedTime == null )
				StartFixedTime = frame.Time;

			// 加入 sync frame 处理队列
			Simulator.ReferencableAllocator.Allocate( frame );
			m_syncFrames.Enqueue( frame );
		}

		void ApplySyncFrames()
		{
			if( m_syncFrames.Count == 0 )
				return;

			while( m_syncFrames.Count > 0 )
			{
				// 移除 sync frame 处理队列
				var frame = m_syncFrames.Dequeue();

				m_syncTickContext = new TickContext( TickContextType.Sync, frame.Time );
				EnterContext( m_syncTickContext );

				if( frame is FullSyncFrame fsf )
					ApplyFullSyncFrame( fsf );
				else if( frame is DeltaSyncFrame dsf )
					ApplyDeltaSyncFrame( dsf );
				else
					throw new NotSupportedException( frame.ToString() );

				LeaveContext();

				// 移除 sync frame 处理队列
				frame.Release();
			}

			CleanUpSyncSnapshots();
		}

		void ApplyFullSyncFrame(FullSyncFrame frame)
		{
			//Simulator.Context.Log( "{0}|ApplyFullSyncFrame {1}", Simulator.FixedTime, frame.Time );

			foreach( var es in frame.Entities )
			{
				Simulator.SceneManager.Scene.CreateEntity( es.Id, es.Settings );

				foreach( ComponentSnapshot cs in es.Components )
				{
					var component = Simulator.SceneManager.FindComponent( cs.ComponentId );
					if( component == null )
						Simulator.Context.LogError( "Failed to find component '{0}'", cs.ComponentId );
					else
						component.RecoverSnapshot( cs );
				}
			}
		}

		void ApplyDeltaSyncFrame(DeltaSyncFrame frame)
		{
			//Simulator.Context.Log( "{0}|ApplyDeltaSyncFrame {1}", Simulator.FixedTime, frame.Time );

			foreach( Event e in frame.Events )
			{
				e.Retain();

				if( e is SceneEvent se )
				{
					Simulator.SceneManager.Scene.ApplyEvent( se );
				}
				else if( e is ComponentEvent ce )
				{
					var component = Simulator.SceneManager.FindComponent( ce.ComponentId );
					if( component == null )
						Simulator.Context.LogError( "Failed to find component '{0}'", ce.ComponentId );
					else
						component.ApplyEvent( ce );

					m_syncedComponents.Add( component );
				}
				else
					throw new NotSupportedException( e.ToString() );
			}
		}

		void CleanUpSyncSnapshots()
		{
			// 清理冗余的 Sync Timeline
			var expiration = (uint)Math.Round( Simulator.SynchronizedClock.Rtt / 2f * 1000f + Simulator.InterpolationManager.InterpolationDelay * 2 );
			if( Simulator.FixedTime > expiration )
			{
				var context = new TickContext( TickContextType.Sync, Simulator.FixedTime - expiration );

				foreach( var component in m_syncedComponents )
					component.RemoveStatesBefore( context );
			}

			m_syncedComponents.Clear();
		}

		#endregion

		#region Reconciliation

		void ReconcilePredictions()
		{
			// 没有新的同步帧，或没有新的预测帧需要和解
			if( m_syncTickContext.Time <= m_reconciliationTickContext.Time ||
				m_predictionTickContext.Time < m_syncTickContext.Time )
			{
				//Simulator.Context.Log( "{0}|No need to reconcile #1 Sync: {1}, Reconciliation: {2}, Prediction: {3}",
				//	Simulator.FixedTime, m_syncTickContext.Time, m_reconciliationTickContext.Time, m_predictionTickContext.Time );

				return;
			}

			// 更新和解时间
			m_reconciliationTickContext = new TickContext( TickContextType.Reconciliation, m_syncTickContext.Time );

			var components = m_predictiveComponents;

			// 判断是否需要和解
			if( !RequireReconciliation( components ) )
			{
				//Simulator.Context.Log( "{0}|All appromiate", Simulator.FixedTime );

				CleanUpAcknowledgedCommands();
				CleanUpPredictionSnapshots( components );

				return;
			}

			// 回滚到同步状态
			//Simulator.Context.LogWarning( "{0}|Rollback snapshot to reconciliation {1} -> {2}", Simulator.FixedTime, m_predictionTickContext.Time, m_reconciliationTickContext.Time );

			EnterContext( m_reconciliationTickContext );
			foreach( var component in components )
			{
				var syncState = component.GetState( m_syncTickContext );
				component.RecoverSnapshot( syncState );
			}
			LeaveContext();

			// 以和解模式更新到最新预测的状态
			while( m_reconciliationTickContext.Time < m_predictionTickContext.Time )
			{
				m_reconciliationTickContext = new TickContext( TickContextType.Reconciliation, m_reconciliationTickContext.Time + Configuration.SimulationDeltaTime );
				//Simulator.Context.Log( "{0}|Simulate for reconciliation {1}", Simulator.FixedTime, m_reconciliationTickContext.Time );

				EnterContext( m_reconciliationTickContext );
				DispatchCommands( m_reconciliationTickContext );
				FixedUpdate();
				LeaveContext();
			}

			// 和解最新预测的状态
			//Simulator.Context.LogWarning( "{0}|Recover prediction snapshot {1}", Simulator.FixedTime, m_predictionTickContext.Time );

			EnterContext( m_predictionTickContext );
			foreach( var component in components )
			{
				var reconciliationState = component.GetState( m_reconciliationTickContext );
				var predictionState = component.GetState( m_predictionTickContext );

				// 判断是需要修正为和解后的状态
				if( reconciliationState.IsApproximate( predictionState ) )
					continue;

				// 以和解后的状态和最新预测的状态的中间值，来纠正最新的预测
				if( Configuration.ComponentReconciliationRatio < 1 )
					reconciliationState = predictionState.Interpolate( reconciliationState, Configuration.ComponentReconciliationRatio );

				component.RecoverSnapshot( reconciliationState, isReconciliation: true );

				CleanUpReconciliationSnapshots( component );
			}
			LeaveContext();

			// 重置和解时间
			m_reconciliationTickContext = new TickContext( TickContextType.Reconciliation, m_syncTickContext.Time );

			// 清理已确认命令
			CleanUpAcknowledgedCommands();
		}

		bool RequireReconciliation(List<Component> components)
		{
			var predictionContext = new TickContext( TickContextType.Prediction, m_reconciliationTickContext.Time );

			foreach( var component in components )
			{
				var predictionState = component.GetState( predictionContext );
				if( predictionState == null )
					return false;

				var syncState = component.GetState( m_syncTickContext );
				if( !syncState.IsApproximate( predictionState ) )
				{
					Simulator.Context.LogWarning( "Found prediction error. Prediction: {0}, {1}, Sync: {2}, {3}",
						predictionContext.Time, predictionState, m_syncTickContext.Time, syncState );

					return true;
				}
			}

			return false;
		}

		void CleanUpAcknowledgedCommands()
		{
			Simulator.CommandQueue.RemoveBefore( Simulator.LocalUserId.Value, m_syncTickContext.Time );
		}

		void CleanUpPredictionSnapshots(List<Component> components)
		{
			// 清理**预测**时间轴
			var context = new TickContext( TickContextType.Prediction, m_reconciliationTickContext.Time );

			foreach( var component in components )
				component.RemoveStatesBefore( context );
		}

		void CleanUpReconciliationSnapshots(Component component)
		{
			// TODO 完全清理**纠正**时间轴
		}

		#endregion

		#region Prediction

		void Predict()
		{
			m_predictionTickContext = new TickContext( TickContextType.Prediction, Simulator.FixedTime );

			EnterContext( m_predictionTickContext );

			Simulator.InputManager.SetInput();

			var f = Simulator.InputManager.CreateCommands();
			m_commandFrames.Enqueue( f );
			f.Retain();

			DispatchCommands( m_predictionTickContext );
			FixedUpdate();

			Simulator.InputManager.ResetInput();

			LeaveContext();
		}

		internal void AddPredictiveComponents(Component component)
		{
			if( !m_predictiveComponents.Contains( component ) )
				m_predictiveComponents.Add( component );
		}

		internal void RemovePredictiveComponents(Component component)
		{
			m_predictiveComponents.Remove( component );
		}

		void DispatchCommands(TickContext ctx)
		{
			var frame = Simulator.CommandQueue.Find( Simulator.LocalUserId.Value, ctx.Time );

			DispatchCommands( frame );
		}

		#endregion

		#region Interpolation

		internal void Interpolate()
		{
		}

		#endregion

		public CommandFrame FetchCommandFrame()
		{
			if( m_commandFrames.Count > 0 )
				return m_commandFrames.Dequeue();
			else
				return null;
		}

		internal CommandFrame FetchCommandFrame2()
		{
			var frame = Simulator.CommandQueue.Find( Simulator.LocalUserId.Value, m_predictionTickContext.Time );
			frame.Retain();
			return frame;
		}
	}
}
