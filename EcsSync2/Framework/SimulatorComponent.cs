namespace EcsSync2
{
	public abstract class SimulatorComponent
	{
		public Simulator Simulator { get; private set; }

		internal virtual void OnInitialize(Simulator simulator)
		{
			Simulator = simulator;
		}

		internal virtual void OnStart() { }

		internal virtual void OnUpdate() { }

		internal virtual void OnFixedUpdate() { }

		internal virtual void OnLateUpdate() { }
	}
}
