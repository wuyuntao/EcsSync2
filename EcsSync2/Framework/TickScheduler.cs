using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public abstract class TickScheduler : SimulatorComponent
	{
		public enum TickContextType
		{
			Sync,
			Reconciliation,
			Prediction,
			Interpolation,
		}

		internal struct TickContext : IEquatable<TickContext>
		{
			public readonly TickContextType Type;

			public readonly uint LocalTime;

			public readonly uint RemoteTime;

			public TickContext(TickContextType type, uint time)
			{
				Type = type;
				LocalTime = RemoteTime = time;
			}

			public TickContext(TickContextType type, uint localTime, uint remoteTime)
			{
				Type = type;
				LocalTime = localTime;
				RemoteTime = remoteTime;
			}

			public override string ToString()
			{
				return $"{nameof( TickContext )}({Type}:L{LocalTime}/R{RemoteTime})";
			}

			public override int GetHashCode()
			{
				return Type.GetHashCode() ^ LocalTime.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				if( obj is TickContext tc )
					return Equals( tc );
				else
					return false;
			}

			public bool Equals(TickContext other)
			{
				return Type == other.Type && LocalTime == other.LocalTime;
			}

			public static bool operator ==(TickContext ctx1, TickContext ctx2)
			{
				return ctx1.Equals( ctx2 );
			}

			public static bool operator !=(TickContext ctx1, TickContext ctx2)
			{
				return !ctx1.Equals( ctx2 );
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
		}

		internal void EnterContext(TickContext context)
		{
			CurrentContext = context;
		}

		internal void LeaveContext()
		{
			CurrentContext = null;
		}

		internal abstract void Tick();

		protected void DispatchCommands(CommandFrame frame)
		{
			if( frame.Commands.Count > 0 )
			{
				//Simulator.Context.Log( "DispatchCommands time {0}, {1} commands", frame.Time, frame.Commands.Count );

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
			var clean = false;

			for( int i = 0; i < Components.Count; i++ )
			{
				var c = Components[i];
				if( c == null )
					continue;

				if( c.Disposed )
				{
					clean = true;
					Components[i] = null;
				}

				c.FixedUpdate();
			}

			if( clean )
				Components.RemoveAll( c => c == null || c.Disposed );
		}
	}
}
