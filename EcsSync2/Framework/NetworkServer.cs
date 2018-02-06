using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public class NetworkServer : NetworkManager
	{
		public interface INetworkServer : INetworkManager
		{
			void Bind(int port);
		}

		public interface IContext
		{
			INetworkServer CreateServer();

			LoginResult VerifyLogin(ulong userId);

			EntitySettings CreatePlayerSettings(ulong userId);
		}

		#region Session

		class Session : Disposable
		{
			NetworkServer Server { get; }
			public uint Id { get; }
			public INetworkStream Stream { get; }
			public ulong UserId { get; private set; }
			public bool IsNewSession { get; private set; }

			object m_receiveLock = new object();
			List<Message> m_receiveMessages = new List<Message>();

			public Session(NetworkServer server, uint id, INetworkStream stream)
			{
				Server = server;
				Id = id;
				Stream = stream;
				Stream.OnReceived += OnReceived;
			}

			public override string ToString()
			{
				return $"{GetType().Name}-{Id}";
			}

			protected override void DisposeManaged()
			{
				Stream.OnReceived -= OnReceived;

				base.DisposeManaged();
			}

			void OnReceived(Message message)
			{
				//Server.Simulator.Context.Log( "OnReceived {0}, {1}", Stream, message );

				lock( m_receiveLock )
					m_receiveMessages.Add( message );
			}

			public void ReceiveMessages()
			{
				lock( m_receiveLock )
				{
					if( m_receiveMessages.Count > 0 )
					{
						foreach( var message in m_receiveMessages )
							Server.OnReceiveMessage( this, message );

						m_receiveMessages.Clear();
					}
				}
			}

			public void OnLoginSucceeded(ulong userId)
			{
				UserId = userId;
				IsNewSession = true;
			}

			public void OnFullSynchornized()
			{
				IsNewSession = false;
			}
		}

		#endregion

		IContext m_context;
		INetworkServer m_server;
		uint m_maxSessionId;
		object m_sessionLock = new object();
		List<Session> m_sessions = new List<Session>();
		CommandFrame m_commandFrame;

		public NetworkServer(Simulator simulator)
			: base( simulator )
		{
			m_context = (IContext)simulator.Context;
			m_server = m_context.CreateServer();
			m_server.OnConnected += OnConnected;
			m_server.OnDisconnected += OnDisconnected;
		}

		public void Start(int port)
		{
			m_server.Bind( port );
		}

		void OnConnected(INetworkStream stream)
		{
			var session = new Session( this, ++m_maxSessionId, stream );
			lock( m_sessionLock )
			{
				m_sessions.Add( session );
			}

			Simulator.Context.Log( "Server OnConnected {0}", session );
		}

		void OnDisconnected(INetworkStream stream)
		{
			lock( m_sessionLock )
			{
				var index = m_sessions.FindIndex( s => s.Stream == stream );
				if( index < 0 )
					throw new InvalidOperationException( "Session not found" );

				using( var session = m_sessions[index] )
				{
					m_sessions.RemoveAt( index );

					Simulator.Context.Log( "Server OnDisconnected {0}", session );
				}
			}
		}

		internal override void ReceiveMessages()
		{
			m_server.Poll();

			lock( m_sessionLock )
			{
				foreach( var session in m_sessions )
					session.ReceiveMessages();
			}

			EnqueueCommands();
		}

		void EnqueueCommands()
		{
			if( m_commandFrame != null )
			{
				m_commandFrame.Time = Simulator.ServerTickScheduler.Time + Configuration.SimulationDeltaTime;

				Simulator.CommandQueue.Add( 0, m_commandFrame );

				m_commandFrame.Release();
				m_commandFrame = null;
			}
		}

		void OnReceiveMessage(Session session, Message message)
		{
			//Simulator.Context.Log( "OnReceiveMessage {0}, {1}", session, message );

			switch( message )
			{
				case LoginRequest m:
					OnLoginRequest( session, m );
					break;

				case HeartbeatRequest m:
					OnHeartbeatRequest( session, m );
					break;

				case CommandFrame m:
					OnCommandFrame( session, m );
					break;

				default:
					throw new NotSupportedException( message.ToString() );
			}
		}

		void OnLoginRequest(Session session, LoginRequest req)
		{
			var status = m_context.VerifyLogin( req.UserId );
			if( status == LoginResult.Ok )
			{
				session.OnLoginSucceeded( req.UserId );
				Simulator.Context.Log( "OnLoginRequest {0}, {1}", session, req );

				var res = new LoginResponse()
				{
					Result = status,
					ClientTime = req.ClientTime,
					ServerTime = (uint)Math.Round( Simulator.SynchronizedClock.LocalTime * 1000 )
				};
				session.Stream.Send( res );

				var f = EnsureCommandFrame();
				var c = f.AddCommand<CreateEntityCommand>();
				c.Settings = m_context.CreatePlayerSettings( req.UserId );
			}
			else
			{
				Simulator.Context.Log( "OnLoginRequest {0}, {1}", req, status );

				var res = new LoginResponse()
				{
					Result = status,
				};
				session.Stream.Send( res );
			}
		}

		void OnHeartbeatRequest(Session session, HeartbeatRequest req)
		{
			var res = new HeartbeatResponse()
			{
				ClientTime = req.ClientTime,
				ServerTime = (uint)Math.Round( Simulator.SynchronizedClock.LocalTime * 1000 ),
			};
			session.Stream.Send( res );
		}

		void OnCommandFrame(Session session, CommandFrame frame)
		{
			//Simulator.Context.Log( "OnCommandFrame {0}, {1}", session, frame );

			Simulator.ReferencableAllocator.Allocate( frame );
			Simulator.CommandQueue.Add( session.UserId, frame );
		}

		CommandFrame EnsureCommandFrame()
		{
			if( m_commandFrame == null )
				m_commandFrame = Simulator.ReferencableAllocator.Allocate<CommandFrame>();
			return m_commandFrame;
		}

		internal override void SendMessages()
		{
			FullSyncFrame fullSyncFrame = null;
			DeltaSyncFrame deltaSyncFrame = null;

			foreach( var session in m_sessions )
			{
				if( session.IsNewSession )
				{
					if( fullSyncFrame == null )
						fullSyncFrame = Simulator.ServerTickScheduler.FetchFullSyncFrame();

					session.Stream.Send( fullSyncFrame );
					session.OnFullSynchornized();
				}
				else if( session.UserId > 0 )
				{
					if( deltaSyncFrame == null )
						deltaSyncFrame = Simulator.ServerTickScheduler.FetchDeltaSyncFrame();

					session.Stream.Send( deltaSyncFrame );
				}
			}

			fullSyncFrame?.Release();
			deltaSyncFrame?.Release();
		}
	}
}
