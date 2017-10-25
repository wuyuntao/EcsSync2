namespace EcsSync2
{
	public abstract class SimulatorComponent
	{
		public Simulator Simulator { get; }

		protected SimulatorComponent(Simulator simulator)
		{
			Simulator = simulator;
		}
	}
}
