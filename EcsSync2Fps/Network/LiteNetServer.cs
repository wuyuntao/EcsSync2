using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.IO;

namespace EcsSync2.Fps
{
	public sealed class LiteNetServer : NetworkServer.IServerContext
	{
		const string ConnectKey = "EcsSync2";

		public Action<NetworkManager.IStream> OnConnected { get; set; }
		public Action<NetworkManager.IStream> OnDisconnected { get; set; }

		public ILogger Logger { get; }

		NetManager m_netManager;
		NetPeerWriter m_netPeerWriter;

		public LiteNetServer(ILogger logger)
		{
			Logger = logger;
			m_netPeerWriter = new NetPeerWriter( logger );

			var listener = new EventBasedNetListener();
			listener.PeerConnectedEvent += Listener_PeerConnectedEvent;
			listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;
			listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
			listener.NetworkErrorEvent += Listener_NetworkErrorEvent;

			m_netManager = new NetManager( listener, 100, ConnectKey )
			{
				MergeEnabled = true,
			};
		}

		public void Bind(int port)
		{
			m_netManager.Start( port );
		}

		public void Poll()
		{
			m_netManager.PollEvents();
		}

		void Listener_PeerConnectedEvent(NetPeer peer)
		{
			var stream = new LiteNetStream( peer, m_netPeerWriter );
			peer.Tag = stream;
			OnConnected?.Invoke( stream );

			Logger?.Log( "Listener_PeerConnectedEvent {0}", peer );
		}

		void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			var stream = (NetworkManager.IStream)peer.Tag;
			OnDisconnected?.Invoke( stream );

			Logger?.Log( "Listener_PeerDisconnectedEvent {0}, {1}", peer, disconnectInfo );
		}

		void Listener_NetworkReceiveEvent(NetPeer peer, NetDataReader reader)
		{
			using( var ms = new MemoryStream( reader.Data ) )
			{
				var env = Serializers.Deserialize<MessageEnvelop>( ms );

				var stream = (NetworkManager.IStream)peer.Tag;
				stream.OnReceived?.Invoke( env.Message );
			}
		}

		void Listener_NetworkErrorEvent(NetEndPoint endPoint, int socketErrorCode)
		{
			Logger?.LogError( "Listener_NetworkErrorEvent {0}, {1}", endPoint, socketErrorCode );
		}
	}
}
