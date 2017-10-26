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

			public uint DeltaTime { get => Settings.SimulationDeltaTime; }

			public TickContext(TickContextType type)
			{
				Type = type;
			}
		}

		public TickContext CurrentContext { get; private set; }
		public List<Tickable> Tickables { get; } = new List<Tickable>();

		protected TickScheduler(Simulator simulator)
			: base( simulator )
		{
		}

		internal void AddTickable(Tickable tickable)
		{
			Tickables.Add( tickable );
		}

		internal void EnterContext(TickContext context)
		{
			CurrentContext = context;

			foreach( var t in Tickables )
				t.EnterContext( context );
		}

		internal abstract void Tick();

		protected void DispatchCommands(CommandFrame frame)
		{
			if( frame.Commands != null )
			{
				foreach( var command in frame.Commands )
				{
					var c = Simulator.SceneManager.FindComponent( command.Receiver );
					c.ReceiveCommand( command );
				}
			}
		}

		internal void FixedUpdate()
		{
			foreach( var t in Tickables )
				t.FixedUpdate();
		}

		internal void LeaveContext()
		{
			foreach( var t in Tickables )
				t.LeaveContext();

			CurrentContext = null;
		}
	}
}
