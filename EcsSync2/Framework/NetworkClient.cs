using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public class NetworkClient : NetworkManager
	{
		public interface IClientContext : IContext
		{
			void Connect(string address, int port);
		}

		public Action OnLogin;

		IClientContext m_client;
		IStream m_stream;

		object m_receiveLock = new object();
		List<Message> m_receiveMessages = new List<Message>();
		float m_lastHeartbeatTime;

		public NetworkClient(Simulator simulator)
			: base( simulator )
		{
			m_client = (IClientContext)simulator.Context;
			m_client.OnConnected += OnConnected;
			m_client.OnDisconnected += OnDisconnected;
		}

		public void Start(string address, int port)
		{
			m_client.Connect( address, port );
		}

		void OnConnected(IStream stream)
		{
			m_stream = stream;
			m_stream.OnReceived += OnReceived;

			Simulator.Context.Log( "Client OnConnected {0}", m_stream );

			var req = new LoginRequest()
			{
				UserId = Simulator.LocalUserId.Value,
				ClientTime = (uint)Math.Round( Simulator.SynchronizedClock.LocalTime * 1000 ),
			};

			m_stream.Send( req );
		}

		void OnDisconnected(IStream stream)
		{
			if( m_stream == stream )
			{
				Simulator.Context.Log( "Client OnDisconnected {0}", m_stream );

				m_stream.OnReceived -= OnReceived;
				m_stream = null;
			}
		}

		void OnReceived(Message message)
		{
			//Simulator.Context.Log( "Client OnReceived {0}", message );

			lock( m_receiveLock )
				m_receiveMessages.Add( message );
		}

		internal override void ReceiveMessages()
		{
			m_client.Poll();

			lock( m_receiveLock )
			{
				if( m_receiveMessages.Count > 0 )
				{
					foreach( var message in m_receiveMessages )
						OnReceiveMessage( message );

					m_receiveMessages.Clear();
				}
			}
		}

		void OnReceiveMessage(Message message)
		{
			switch( message )
			{
				case LoginResponse m:
					OnLoginResponse( m );
					break;

				case HeartbeatResponse m:
					OnHeartbeat( m );
					break;

				case FullSyncFrame m:
					OnFullSyncFrame( m );
					break;

				case DeltaSyncFrame m:
					OnDeltaSyncFrame( m );
					break;

				default:
					throw new NotSupportedException( message.ToString() );
			}
		}

		void OnLoginResponse(LoginResponse res)
		{
			var rtt = Simulator.SynchronizedClock.LocalTime - res.ClientTime / 1000f;
			Simulator.SynchronizedClock.Synchronize( res.ServerTime / 1000f, rtt );

			OnLogin?.Invoke();
		}

		void OnHeartbeat(HeartbeatResponse res)
		{
			var rtt = Simulator.SynchronizedClock.LocalTime - res.ClientTime / 1000f;
			Simulator.SynchronizedClock.Synchronize( res.ServerTime / 1000f, rtt );
		}

		void OnFullSyncFrame(FullSyncFrame frame)
		{
			Simulator.ClientTickScheduler.ReceiveSyncFrame( frame );
		}

		void OnDeltaSyncFrame(DeltaSyncFrame frame)
		{
			Simulator.ClientTickScheduler.ReceiveSyncFrame( frame );
		}

		internal override void SendMessages()
		{
			if( m_stream != null )
			{
				var commandFrame = Simulator.ClientTickScheduler.FetchCommandFrame();
				if( commandFrame != null )
				{
					m_stream.Send( commandFrame );
					commandFrame.Release();

					//Simulator.Context.Log( "Send {0}", commandFrame );
				}

				if( Simulator.SynchronizedClock.LocalTime - m_lastHeartbeatTime > Configuration.HeartbeatIntervalTime / 1000f )
				{
					var req = new HeartbeatRequest()
					{
						ClientTime = (uint)Math.Round( Simulator.SynchronizedClock.LocalTime * 1000 )
					};
					m_stream.Send( req );

					m_lastHeartbeatTime = Simulator.SynchronizedClock.LocalTime;
				}
			}
		}
	}
}
