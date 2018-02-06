using System;

namespace EcsSync2
{
	public abstract class NetworkManager : SimulatorComponent
	{
		public interface INetworkManager
		{
			Action<INetworkStream> OnConnected { get; set; }

			Action<INetworkStream> OnDisconnected { get; set; }

			void Poll();
		}

		public interface INetworkStream
		{
			Action<Message> OnReceived { get; set; }

			void Send(Message message);
		}

		protected NetworkManager(Simulator simulator)
			: base( simulator )
		{
		}

		internal abstract void ReceiveMessages();

		internal abstract void SendMessages();
	}
}
