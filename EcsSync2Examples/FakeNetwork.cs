using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace EcsSync2.Examples
{
	class FakeNetwork : INetwork
	{
		uint m_maxId;
		Server m_server;
		List<Client> m_clients = new List<Client>();

		ConcurrentQueue<Tuple<Action, float>> m_delayedActions = new ConcurrentQueue<Tuple<Action, float>>();
		float m_time;

		public NetworkServer.INetworkServer CreateServer()
		{
			m_server = new Server( this, ++m_maxId );
			return m_server;
		}

		public NetworkClient.INetworkClient CreateClient()
		{
			var client = new Client( this, ++m_maxId );
			m_clients.Add( client );
			return client;
		}

		NetworkManager.INetworkStream CreateStream(Client client, bool clientSide)
		{
			return new Stream( this, ++m_maxId, client, clientSide );
		}

		void EnqueueAction(Action action, float delay)
		{
			m_delayedActions.Enqueue( Tuple.Create( action, m_time + delay ) );
		}

		public void InvokeActions(float time)
		{
			m_time = time;

			while( m_delayedActions.TryPeek( out Tuple<Action, float> r ) && r.Item2 <= m_time )
			{
				m_delayedActions.TryDequeue( out r );
				r.Item1.Invoke();
			}
		}

		abstract class BaseObject
		{
			public FakeNetwork Network { get; }
			public uint Id { get; }

			public BaseObject(FakeNetwork network, uint id)
			{
				Network = network;
				Id = id;
			}

			public override string ToString()
			{
				return $"{GetType().Name}-{Id}";
			}
		}

		class Server : BaseObject, NetworkServer.INetworkServer
		{
			public Action<NetworkManager.INetworkStream> OnConnected { get; set; }
			public Action<NetworkManager.INetworkStream> OnDisconnected { get; set; }

			public Server(FakeNetwork network, uint id)
				: base( network, id )
			{
			}

			public void Bind(int port)
			{
				Logger.Log( "Bind {0}", port );
			}

			public void Poll()
			{
			}
		}

		class Client : BaseObject, NetworkClient.INetworkClient
		{
			public Action<NetworkManager.INetworkStream> OnConnected { get; set; }
			public Action<NetworkManager.INetworkStream> OnDisconnected { get; set; }

			public float Rtt { get; } = 0.1f;

			public NetworkManager.INetworkStream ClientStream { get; private set; }
			public NetworkManager.INetworkStream ServerStream { get; private set; }

			public Client(FakeNetwork network, uint id)
				: base( network, id )
			{
			}

			public void Connect(string address, int port)
			{
				Logger.Log( "Connect {0}:{1}", address, port );

				var clientStream = Network.CreateStream( this, true );
				var serverStream = Network.CreateStream( this, false );

				Network.EnqueueAction( () =>
				{
					Network.m_server.OnConnected?.Invoke( serverStream );
				}, Rtt / 2 );

				Network.EnqueueAction( () =>
				{
					OnConnected?.Invoke( clientStream );
				}, Rtt );

				ClientStream = clientStream;
				ServerStream = serverStream;
			}

			public void Poll()
			{
			}
		}

		class Stream : BaseObject, NetworkManager.INetworkStream
		{
			public Action<Message> OnReceived { get; set; }

			public Client Client { get; }
			public bool ClientSide { get; }

			byte[] m_buffer = new byte[1024];

			public Stream(FakeNetwork network, uint id, Client client, bool clientSide)
				: base( network, id )
			{
				Client = client;
				ClientSide = clientSide;
			}

			public override string ToString()
			{
				return $"{GetType().Name}-{Client}-{ClientSide}";
			}

			public void Send(Message message)
			{
				var env = new MessageEnvelop { Message = message };
				var len = 0;
				using( var ms1 = new MemoryStream( m_buffer ) )
				{
					Serializers.Serialize( ms1, env );
					len = (int)ms1.Position;
				}

				using( var ms2 = new MemoryStream( m_buffer, 0, len ) )
				{
					env = Serializers.Deserialize<MessageEnvelop>( ms2 );
					message = env.Message;
				}

				//Logger.Log( "{0} send {1}", this, message );

				var stream = Client.ClientStream == this ?
					Client.ServerStream :
					Client.ClientStream;

				Network.EnqueueAction( () =>
				{
					stream.OnReceived?.Invoke( message );
				}, Client.Rtt / 2f );
			}
		}
	}
}
