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

		List<Component> m_syncedComponents = new List<Component>();
		List<Component> m_predictiveComponents = new List<Component>();
		Queue<SyncFrame> m_syncFrames = new Queue<SyncFrame>();
		internal uint? FullSyncTime { get; private set; }

		public ClientTickScheduler(Simulator simulator)
			: base( simulator )
		{
		}

		internal override void Tick()
		{
			Simulator.NetworkClient.ReceiveMessages();
			ApplySyncFrames();

			if( FullSyncTime == null )        // Not synchronizied yet
				return;

			ReconcilePredictions();

			for( int i = 0; i < Configuration.MaxTickCount; i++ )
			{
				var deltaTime = Configuration.SimulationDeltaTime / 1000f;
				var nextTime = m_predictionTickContext.LocalTime / 1000f + deltaTime;
				// 增加预测的提前时间 RTT / 2 + DeltaTime * 2，比暴雪的算法多了一帧
				// 可能是因为服务端模拟器的理念是提前一帧更新（表示未来的一帧内发生的事情，即没有 Time = 0 的逻辑帧）
				var predictionTime = Simulator.SynchronizedClock.Time + Simulator.SynchronizedClock.Rtt / 2f + deltaTime * 2;
				if( predictionTime < nextTime )
					break;

				Predict();

				//Simulator.Context.Log( "Tick sync: {0}, predict: {1}", m_syncTickContext.Time, m_predictionTickContext.Time );
			}
		}

		#region Synchronization

		public void ReceiveSyncFrame(SyncFrame frame)
		{
			Debug.Assert( frame.Time != 0 );

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
			FullSyncTime = frame.Time;
			m_reconciliationTickContext = new TickContext( TickContextType.Reconciliation, frame.Time );
			m_predictionTickContext = new TickContext( TickContextType.Prediction, frame.Time );

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

			foreach( var cs in frame.Clocks )
			{
				if( cs.UserId == Simulator.LocalUserId )
				{
					Simulator.SynchronizedClock.SpeedUp = cs.SpeedUp;
					break;
				}
			}
		}

		void CleanUpSyncSnapshots()
		{
			if( Simulator.RenderManager.CurrentContext == null )
				return;

			if( Simulator.RenderManager.CurrentContext.Value.RemoteTime < Configuration.SimulationDeltaTime )
				return;

			// 清理冗余的 Sync Timeline
			var context = new TickContext( TickContextType.Sync, Simulator.RenderManager.CurrentContext.Value.RemoteTime - Configuration.SimulationDeltaTime );

			foreach( var component in m_syncedComponents )
				component.RemoveStatesBefore( context );

			m_syncedComponents.Clear();
		}

		#endregion

		#region Reconciliation

		void ReconcilePredictions()
		{
			// 没有新的同步帧，或没有新的预测帧需要和解
			if( m_syncTickContext.LocalTime <= m_reconciliationTickContext.LocalTime ||
				m_predictionTickContext.LocalTime < m_syncTickContext.LocalTime )
			{
				//Simulator.Context.Log( "{0}|No need to reconcile #1 Sync: {1}, Reconciliation: {2}, Prediction: {3}",
				//	Simulator.FixedTime, m_syncTickContext.Time, m_reconciliationTickContext.Time, m_predictionTickContext.Time );

				return;
			}

			// 更新和解时间
			m_reconciliationTickContext = new TickContext( TickContextType.Reconciliation, m_syncTickContext.LocalTime );

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
			while( m_reconciliationTickContext.LocalTime < m_predictionTickContext.LocalTime )
			{
				m_reconciliationTickContext = new TickContext( TickContextType.Reconciliation, m_reconciliationTickContext.LocalTime + Configuration.SimulationDeltaTime );
				//Simulator.Context.Log( "{0}|Simulate for reconciliation {1}", Simulator.FixedTime, m_reconciliationTickContext.Time );

				EnterContext( m_reconciliationTickContext );
				DispatchCommands( m_reconciliationTickContext );
				FixedUpdate();
				LeaveContext();
			}

			// 和解所有时间点的预测的状态
			ReconcilePredictiveComponents( components );

			// 重置和解时间
			m_reconciliationTickContext = new TickContext( TickContextType.Reconciliation, m_syncTickContext.LocalTime );

			// 清理已确认命令和和解快照
			CleanUpAcknowledgedCommands();
			CleanUpReconciliationSnapshots();
		}

		bool RequireReconciliation(List<Component> components)
		{
			var predictionContext = new TickContext( TickContextType.Prediction, m_reconciliationTickContext.LocalTime );

			foreach( var component in components )
			{
				var predictionState = component.GetState( predictionContext );
				if( predictionState == null )
					return false;

				var syncState = component.GetState( m_syncTickContext );
				if( !syncState.IsApproximate( predictionState ) )
				{
					Simulator.Context.LogWarning( "Found prediction error of '{4}'({5}). Prediction: {0}, {1}, Sync: {2}, {3}",
						predictionContext.LocalTime, predictionState, m_syncTickContext.LocalTime, syncState, component.Entity, component.Entity.IsLocalEntity );

					return true;
				}
			}

			return false;
		}

		void ReconcilePredictiveComponents(List<Component> components)
		{
			var predictionTickContext = new TickContext( TickContextType.Prediction, m_reconciliationTickContext.LocalTime );
			//Simulator.Context.LogWarning( "{0}|Recover prediction snapshot {1}", Simulator.FixedTime, predictionTickContext.Time );
			EnterContext( predictionTickContext );
			foreach( var component in components )
			{
				var reconciliationState = component.GetState( m_reconciliationTickContext );
				var predictionState = component.GetState( predictionTickContext );

				// 判断是需要修正为和解后的状态
				if( reconciliationState.IsApproximate( predictionState ) )
					continue;

				// 以和解后的状态和最新预测的状态的中间值，来纠正最新的预测
				//if( component is Fps.Transform && Configuration.ComponentReconciliationRatio < 1 )
				//	reconciliationState = predictionState.Interpolate( reconciliationState, Configuration.ComponentReconciliationRatio );

				component.RecoverSnapshot( reconciliationState );
			}
			LeaveContext();
		}

		void CleanUpAcknowledgedCommands()
		{
			Simulator.CommandQueue.RemoveBefore( Simulator.LocalUserId.Value, m_syncTickContext.LocalTime );
		}

		void CleanUpPredictionSnapshots(List<Component> components)
		{
			if( Simulator.RenderManager.CurrentContext == null )
				return;

			if( Simulator.RenderManager.CurrentContext.Value.RemoteTime < Configuration.SimulationDeltaTime )
				return;

			// 清理**预测**时间轴
			var context = new TickContext( TickContextType.Prediction, Simulator.RenderManager.CurrentContext.Value.RemoteTime - Configuration.SimulationDeltaTime );

			foreach( var component in components )
				component.RemoveStatesBefore( context );
		}

		void CleanUpReconciliationSnapshots()
		{
			// TODO 完全清理**纠正**时间轴
		}

		#endregion

		#region Prediction

		void Predict()
		{
			m_predictionTickContext = new TickContext( TickContextType.Prediction, m_predictionTickContext.LocalTime + Configuration.SimulationDeltaTime );

			EnterContext( m_predictionTickContext );

			Simulator.InputManager.SetInput();
			Simulator.InputManager.CreateCommands();

			DispatchCommands( m_predictionTickContext );
			FixedUpdate();
			Simulator.EventDispatcher.Dispatch();

			Simulator.InputManager.ResetInput();

			LeaveContext();

			Simulator.NetworkClient.SendMessages();
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
			var frame = Simulator.CommandQueue.Find( Simulator.LocalUserId.Value, ctx.LocalTime );

			DispatchCommands( frame );
		}

		#endregion

		#region Interpolation

		internal void Interpolate()
		{
		}

		#endregion

		internal CommandFrame FetchCommandFrame()
		{
			var frame = Simulator.CommandQueue.Find( Simulator.LocalUserId.Value, m_predictionTickContext.LocalTime );
			frame.Retain();
			return frame;
		}
	}
}
