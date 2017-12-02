using System;

namespace EcsSync2.Examples
{
	static class Logger
	{
		static object s_lock = new object();

		public static void Log(string msg, params object[] args)
		{
			lock( s_lock )
			{
				Console.ForegroundColor = ConsoleColor.DarkCyan;
				Console.Write( "[DEBUG]|{0}|", DateTime.Now.ToString( "HH:mm:ss.fff" ) );
				Console.WriteLine( msg, args );
			}
		}

		public static void LogWarning(string msg, params object[] args)
		{
			lock( s_lock )
			{
				Console.ForegroundColor = ConsoleColor.DarkYellow;
				Console.Write( "[WARNING]|{0}|", DateTime.Now.ToString( "HH:mm:ss.fff" ) );
				Console.WriteLine( msg, args );
			}
		}

		public static void LogError(string msg, params object[] args)
		{
			lock( s_lock )
			{
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.Write( "[ERROR]|{0}|", DateTime.Now.ToString( "HH:mm:ss.fff" ) );
				Console.WriteLine( msg, args );
			}
		}
	}
}
