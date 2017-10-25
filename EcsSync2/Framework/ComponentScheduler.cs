using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public abstract class ComponentScheduler : SimulatorComponent
	{
		protected void DispatchCommands(Component.ITickContext ctx, CommandFrame frame)
		{
			if( frame.Commands != null )
			{
				foreach( var command in frame.Commands )
				{
					var component = Simulator.SceneManager.FindComponent( command.Receiver );
					component.ReceiveCommand( ctx, command );
				}
			}
		}

		protected void FixedUpdateComponents(Component.ITickContext ctx)
		{
		}
	}

	public class ServerComponentScheduler : ComponentScheduler
	{
		TickContext m_tickContext;
		SortedList<ulong, CommandFrame> m_dispatchedCommands = new SortedList<ulong, CommandFrame>();

		internal override void OnInitialize(Simulator simulator)
		{
			base.OnInitialize( simulator );

			m_tickContext = new TickContext( simulator );
		}

		internal override void OnFixedUpdate()
		{
			base.OnFixedUpdate();

			DispatchCommands( m_tickContext );
			FixedUpdateComponents( m_tickContext );
		}

		void DispatchCommands(Component.ITickContext ctx)
		{
			foreach( var player in Simulator.SceneManager.Scene.Players )
			{
				if( player.IsAI )
					continue;

				// TODO 减少按 UserId 的查询
				m_dispatchedCommands.TryGetValue( player.UserId, out CommandFrame lastFrame );

				// 尝试执行从上一次应用的命令帧开始，到当前帧之间的所有命令
				var lastFrameChanged = false;
				for( var time = lastFrame != null ? lastFrame.Time + ctx.DeltaTime : ctx.Time;
					time <= ctx.Time;
					time += ctx.DeltaTime )
				{
					var frame = Simulator.CommandQueue.FetchCommands( player.UserId, time );
					if( frame == null )
						break;

					DispatchCommands( ctx, frame );
					lastFrame = frame;
					lastFrameChanged = true;
				}

				if( lastFrame != null )
				{
					// 更新当前应用的命令帧
					if( lastFrameChanged )
						m_dispatchedCommands[player.UserId] = lastFrame;

					// 如果无法获取当前帧的话，总是重复上一次的命令帧
					if( lastFrame.Time != ctx.Time )
					{
						DispatchCommands( ctx, lastFrame );

						// TODO 按需加速客户端
					}
					else
					{
						// TODO 按需恢复客户端加速
					}

					// 清理已应用命令
					Simulator.CommandQueue.DequeueBefore( player.UserId, lastFrame.Time );
				}
			}
		}

		#region TickContext

		class TickContext : Component.ITickContext
		{
			Simulator m_simulator;

			public TickContext(Simulator simulator)
			{
				m_simulator = simulator;
			}

			uint Component.ITickContext.Time => m_simulator.FixedTime;

			uint Component.ITickContext.DeltaTime => m_simulator.FixedDeltaTime;
		}

		#endregion
	}

	public class ClientComponentScheduler : ComponentScheduler
	{
		TickContext m_syncTickContext = new TickContext();
		TickContext m_reconcilationTickContext = new TickContext();
		TickContext m_predictionTickContext = new TickContext();
		Queue<SyncFrame> m_syncFrames = new Queue<SyncFrame>();

		internal override void OnFixedUpdate()
		{
			base.OnFixedUpdate();

			ApplySyncFrames();
			ReconcilePredictions();
			Predict();
		}

		#region Synchronization

		internal void ReceiveSyncFrame(SyncFrame frame)
		{
			m_syncFrames.Enqueue( frame );
		}

		void ApplySyncFrames()
		{
			while( m_syncFrames.Count > 0 )
			{
				var frame = m_syncFrames.Dequeue();

				m_syncTickContext.Time = frame.Time;

				if( frame is FullSyncFrame fsf )
					ApplyFullSyncFrame( fsf );
				else if( frame is DeltaSyncFrame dsf )
					ApplyDeltaSyncFrame( dsf );
				else
					throw new NotSupportedException();
			}
		}

		void ApplyFullSyncFrame(FullSyncFrame frame)
		{
			foreach( var es in frame.Entities )
			{
				Simulator.SceneManager.CreateEntity( es.Id, es.Settings );

				foreach( var cs in es.Components )
				{
					var component = Simulator.SceneManager.FindComponent( cs.Id );
					component.OnSnapshotRecovered( m_syncTickContext, cs );
				}
			}
		}

		void ApplyDeltaSyncFrame(DeltaSyncFrame frame)
		{
			foreach( var e in frame.Events )
			{
				if( e is SceneEvent se )
				{
					Simulator.SceneManager.Scene.OnEventApplied( m_syncTickContext, se );
				}
				else if( e is ComponentEvent ce )
				{
					var component = Simulator.SceneManager.FindComponent( ce.ComponentId );
					component.ApplyEvent( m_syncTickContext, e );
				}
				else
					throw new NotSupportedException();
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

			var components = Simulator.SceneManager.GetPredictedComponents();

			// 判断是否需要和解
			if( !RequireReconcilation( components ) )
				return;

			// 回滚到同步状态
			foreach( var component in components )
			{
				var syncState = component.GetState( m_syncTickContext );
				component.RecoverSnapshot( m_reconcilationTickContext, syncState );
			}

			// 以和解模式更新到最新预测的状态
			while( m_reconcilationTickContext.Time < m_predictionTickContext.Time )
			{
				DispatchCommands( m_reconcilationTickContext );
				FixedUpdateComponents( m_reconcilationTickContext );

				m_reconcilationTickContext.Time += Settings.SimulationDeltaTime;
			}

			// 以和解后的状态和最新预测的状态的中间值，来纠正最新的预测
			foreach( var component in components )
			{
				var reconcilationState = component.GetState( m_reconcilationTickContext );
				if( Settings.ComponentReconcilationRatio < 1 )
				{
					var predictionState = component.GetState( m_predictionTickContext );
					reconcilationState = predictionState.Interpolate( reconcilationState, Settings.ComponentReconcilationRatio );
				}
				component.RecoverSnapshot( m_predictionTickContext, reconcilationState );
			}

			// 重置和解时间
			m_reconcilationTickContext.Time = m_syncTickContext.Time;

			// 清理已确认命令
			Simulator.CommandQueue.DequeueBefore( Simulator.LocalUserId.Value, m_syncTickContext.Time );
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

			DispatchCommands( m_predictionTickContext );
			FixedUpdateComponents( m_predictionTickContext );
		}

		void DispatchCommands(Component.ITickContext ctx)
		{
			var frame = Simulator.CommandQueue.FetchCommands( Simulator.LocalUserId.Value, ctx.Time );

			DispatchCommands( ctx, frame );
		}

		#endregion

		#region TickContext

		class TickContext : Component.ITickContext
		{
			public uint Time { get; set; }

			public uint DeltaTime => Settings.SimulationDeltaTime;
		}

		#endregion
	}
}
