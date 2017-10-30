using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public abstract class TickScheduler : SimulatorComponent
	{
		public enum TickContextType
		{
			Sync,
			Reconcilation,
			Prediction,
			Interpolation,
		}

		public class TickContext
		{
			public TickContextType Type { get; private set; }

			public uint Time { get; set; }

			public uint DeltaTime { get => Configuration.SimulationDeltaTime; }

			public TickContext(TickContextType type)
			{
				Type = type;
			}
		}

		public TickContext CurrentContext { get; private set; }
		public List<Component> Components { get; } = new List<Component>();

		protected TickScheduler(Simulator simulator)
			: base( simulator )
		{
		}

		internal void AddComponent(Component component)
		{
			Components.Add( component );
		}

		internal void EnterContext(TickContext context)
		{
			CurrentContext = context;

			foreach( var t in Components )
				t.EnterContext( context );
		}

		internal abstract void Tick();

		protected void DispatchCommands(CommandFrame frame)
		{
			if( frame.Commands != null )
			{
				foreach( var command in frame.Commands )
				{
					switch( command )
					{
						case SceneCommand c:
							Simulator.SceneManager.Scene.ReceiveCommand( c );
							break;

						case ComponentCommand c:
							var component = Simulator.SceneManager.FindComponent( c.Receiver );
							component.ReceiveCommand( command );
							break;

						default:
							throw new NotSupportedException( command.ToString() );
					}
				}
			}
		}

		internal void FixedUpdate()
		{
			foreach( var t in Components )
				t.FixedUpdate();
		}

		internal void LeaveContext()
		{
			foreach( var t in Components )
				t.LeaveContext();

			CurrentContext = null;
		}
	}
}
