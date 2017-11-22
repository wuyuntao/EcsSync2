using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public class ClientTickScheduler : TickScheduler
	{
		TickContext m_syncTickContext = new TickContext( TickContextType.Sync, 0 );
		TickContext m_reconcilationTickContext = new TickContext( TickContextType.Reconcilation, 0 );
		TickContext m_predictionTickContext = new TickContext( TickContextType.Prediction, 0 );
		TickContext m_interpolationTickContext = new TickContext( TickContextType.Interpolation, 0 );

		Queue<SyncFrame> m_syncFrames = new Queue<SyncFrame>();
		Queue<CommandFrame> m_commandFrames = new Queue<CommandFrame>();

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
			// TODO 有必要加入引用计数么？
			frame.Retain();

			m_syncFrames.Enqueue( frame );
		}

		void ApplySyncFrames()
		{
			while( m_syncFrames.Count > 0 )
			{
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

				frame.Release();
			}
		}

		void ApplyFullSyncFrame(FullSyncFrame frame)
		{
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

					cs.Release();
				}
			}
		}

		void ApplyDeltaSyncFrame(DeltaSyncFrame frame)
		{
			foreach( Event e in frame.Events )
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
			m_reconcilationTickContext = new TickContext( TickContextType.Reconcilation, m_syncTickContext.Time );

			var components = GetPredictedComponents();

			// 判断是否需要和解
			if( !RequireReconcilation( components ) )
				return;

			// 回滚到同步状态
			EnterContext( m_reconcilationTickContext );
			foreach( var component in components )
			{
				var syncState = component.GetState( m_syncTickContext );
				component.RecoverSnapshot( syncState );
			}
			LeaveContext();

			// 以和解模式更新到最新预测的状态
			while( m_reconcilationTickContext.Time < m_predictionTickContext.Time )
			{
				m_reconcilationTickContext = new TickContext( TickContextType.Reconcilation, m_reconcilationTickContext.Time + Configuration.SimulationDeltaTime );

				EnterContext( m_reconcilationTickContext );
				DispatchCommands( m_reconcilationTickContext );
				FixedUpdate();
				LeaveContext();
			}

			// 和解最新预测的状态
			EnterContext( m_predictionTickContext );
			foreach( var component in components )
			{
				var reconcilationState = component.GetState( m_reconcilationTickContext );
				var predictionState = component.GetState( m_predictionTickContext );

				// 判断是需要修正为和解后的状态
				if( reconcilationState.IsApproximate( predictionState ) )
					continue;

				// 以和解后的状态和最新预测的状态的中间值，来纠正最新的预测
				if( Configuration.ComponentReconcilationRatio < 1 )
					reconcilationState = (ComponentSnapshot)predictionState.Interpolate( reconcilationState, Configuration.ComponentReconcilationRatio );

				component.RecoverSnapshot( reconcilationState );
			}
			LeaveContext();

			// 重置和解时间
			m_reconcilationTickContext = new TickContext( TickContextType.Reconcilation, m_syncTickContext.Time );

			// 清理已确认命令
			Simulator.CommandQueue.RemoveBefore( Simulator.LocalUserId.Value, m_syncTickContext.Time );
		}

		List<Component> GetPredictedComponents()
		{
			return Components;
		}

		bool RequireReconcilation(List<Component> components)
		{
			var predictionContext = new TickContext( TickContextType.Prediction, m_reconcilationTickContext.Time );

			foreach( var component in components )
			{
				var predictionState = component.GetState( predictionContext );
				if( predictionState == null )
					return false;

				var syncState = component.GetState( m_syncTickContext );
				if( !syncState.IsApproximate( predictionState ) )
					return true;
			}

			return false;
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
	}
}
