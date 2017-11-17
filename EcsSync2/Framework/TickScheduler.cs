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

		internal struct TickContext
		{
			public readonly TickContextType Type;

			public readonly uint Time;

			public TickContext(TickContextType type, uint time)
			{
				Type = type;
				Time = time;
			}
		}

		internal TickContext? CurrentContext { get; private set; }
		internal List<Component> Components { get; } = new List<Component>();

		protected TickScheduler(Simulator simulator)
			: base( simulator )
		{
		}

		internal void AddComponent(Component component)
		{
			Components.Add( component );

			if( CurrentContext != null )
				component.EnterContext( CurrentContext.Value );
		}

		internal void EnterContext(TickContext context)
		{
			CurrentContext = context;

			foreach( var component in Components )
				component.EnterContext( context );
		}

		internal abstract void Tick();

		protected void DispatchCommands(CommandFrame frame)
		{
			if( frame.Commands.Count > 0 )
			{
				Simulator.Context.Log( "DispatchCommands time {0}, {1} commands", frame.Time, frame.Commands.Count );

				foreach( var command in frame.Commands )
				{
					switch( command )
					{
						case SceneCommand c:
							Simulator.SceneManager.Scene.ReceiveCommand( c );
							break;

						case ComponentCommand c:
							var component = Simulator.SceneManager.FindComponent( c.ComponentId );
							component.ReceiveCommand( c );
							break;

						default:
							throw new NotSupportedException( command.ToString() );
					}
				}
			}
		}

		internal void FixedUpdate()
		{
			foreach( var component in Components )
				component.FixedUpdate();
		}

		internal void LeaveContext()
		{
			foreach( var component in Components )
				component.LeaveContext();

			CurrentContext = null;
		}
	}
}
