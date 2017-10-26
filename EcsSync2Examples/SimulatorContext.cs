namespace EcsSync2.Examples
{
	class SimulatorContext : Simulator.IContext, InputManager.IContext
	{
		float InputManager.IContext.GetAxis(string name)
		{
			return 0;
		}

		bool InputManager.IContext.GetButton(string name)
		{
			return false;
		}
	}
}
