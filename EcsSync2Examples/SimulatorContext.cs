using System;

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

		void ILogger.Log(string msg, params object[] args)
		{
			Console.Write( "[DEBUG] " );
			Console.WriteLine( msg, args );
		}

		void ILogger.LogError(string msg, params object[] args)
		{
			Console.Write( "[ERROR] " );
			Console.WriteLine( msg, args );
		}

		void ILogger.LogWarning(string msg, params object[] args)
		{
			Console.Write( "[WARNING] " );
			Console.WriteLine( msg, args );
		}
	}
}
