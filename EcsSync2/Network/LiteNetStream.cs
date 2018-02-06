using System;
using LiteNetLib;

namespace EcsSync2
{
	class LiteNetStream : NetworkManager.INetworkStream
	{
		public Action<Message> OnReceived { get; set; }

		NetPeer m_peer;
		NetPeerWriter m_writer;

		public LiteNetStream(NetPeer peer, NetPeerWriter writer)
		{
			m_peer = peer;
			m_writer = writer;
		}

		public void Send(Message message)
		{
			m_writer.Write( m_peer, message );
		}
	}
}