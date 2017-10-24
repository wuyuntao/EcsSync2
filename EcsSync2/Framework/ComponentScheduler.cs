using System.Collections.Generic;

namespace EcsSync2
{
	public abstract class ComponentScheduler : SimulatorComponent
	{
		public interface ITickContext
		{
			uint Time { get; }

			uint DeltaTime { get; }
		}

		Queue<CommandFrame> m_commands = new Queue<CommandFrame>();

		internal void EnqueueCommands(CommandFrame frame)
		{
			frame.Retain();

			m_commands.Enqueue( frame );
		}

		protected void DispatchCommands(ITickContext context)
		{
			while( m_commands.Count > 0 )
			{
				var frame = m_commands.Dequeue();
				if( frame.Commands != null )
				{
					foreach( var command in frame.Commands )
					{
						var component = Simulator.SceneManager.FindComponent( command.Receiver );
						component.ReceiveCommand( context, command );
					}
				}
			}
		}

		internal void RegisterFixedUpdate(Component component)
		{
		}

		protected void FixedUpdateComponents(ITickContext context)
		{
		}
	}

	public class ServerComponentScheduler : ComponentScheduler
	{
		TickContext m_tickContext;

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

		#region TickContext

		class TickContext : ITickContext
		{
			Simulator m_simulator;

			public TickContext(Simulator simulator)
			{
				m_simulator = simulator;
			}

			uint ITickContext.Time => m_simulator.FixedTime;

			uint ITickContext.DeltaTime => m_simulator.FixedDeltaTime;
		}

		#endregion
	}

	public class ClientComponentScheduler : ComponentScheduler
	{
		internal override void OnFixedUpdate()
		{
			base.OnFixedUpdate();
		}
	}
}
