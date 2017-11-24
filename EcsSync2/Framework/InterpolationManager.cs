using System;

namespace EcsSync2
{
	public class InterpolationManager : SimulatorComponent
	{
		public InterpolationManager(Simulator simulator)
			: base( simulator )
		{
		}

		internal void Interpolate()
		{
		}

		public uint InterpolationDelay { get; private set; } = 50;
	}
}
