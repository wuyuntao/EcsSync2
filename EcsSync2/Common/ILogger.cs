namespace EcsSync2
{
	public interface ILogger
	{
		void Log(string msg, params object[] args);

		void LogWarning(string msg, params object[] args);

		void LogError(string msg, params object[] args);
	}
}
