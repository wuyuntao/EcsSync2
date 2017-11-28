using EcsSync2.Fps;
using System;

namespace EcsSync2
{
	public abstract class NetworkComponet : SimulatorComponent
	{
		public interface IContext
		{
			Action<IStream> OnConnected { get; set; }

			Action<IStream> OnDisconnected { get; set; }

			void Poll();
		}

		public interface IStream
		{
			Action<Message> OnReceived { get; set; }

			void Send(Message message);
		}

		protected NetworkComponet(Simulator simulator)
			: base( simulator )
		{
		}

		internal abstract void ReceiveMessages();

		internal abstract void SendMessages();
	}
}
