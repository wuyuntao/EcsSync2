using System;
using System.Threading.Tasks;

namespace EcsSync2.Examples
{
	class Program
	{
		static void Main(string[] args)
		{
			Task.Run( (Action)StandaloneTest.Run );
			//Task.Run( (Action)FakeNetworkTest.Run );

			Console.WriteLine( "Press any key to exit" );
			Console.ReadKey();
		}
	}
}