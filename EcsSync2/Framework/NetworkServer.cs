using EcsSync2.Fps;
using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public class NetworkServer : NetworkComponet
	{
		public interface IServerContext : IContext
		{
			void Bind(int port);
		}

		#region Session

		class Session : Disposable
		{
			NetworkServer Server { get; }
			public uint Id { get; }
			public IStream Stream { get; }
			public ulong UserId { get; private set; }
			public bool IsNewSession { get; private set; }

			object m_receiveLock = new object();
			List<Message> m_receiveMessages = new List<Message>();

			public Session(NetworkServer server, uint id, IStream stream)
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

		IServerContext m_server;
		uint m_maxSessionId;
		object m_sessionLock = new object();
		List<Session> m_sessions = new List<Session>();
		CommandFrame m_commandFrame;

		public NetworkServer(Simulator simulator)
			: base( simulator )
		{
			m_server = (IServerContext)simulator.Context;
			m_server.OnConnected += OnConnected;
			m_server.OnDisconnected += OnDisconnected;
		}

		public void Start(int port)
		{
			m_server.Bind( port );
		}

		void OnConnected(IStream stream)
		{
			var session = new Session( this, ++m_maxSessionId, stream );
			lock( m_sessionLock )
			{
				m_sessions.Add( session );
			}

			Simulator.Context.Log( "Server OnConnected {0}", session );
		}

		void OnDisconnected(IStream stream)
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
				m_commandFrame.Time = Simulator.FixedTime + Configuration.SimulationDeltaTime;

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
				case LoginRequestMessage m:
					OnLoginRequest( session, m );
					break;

				case HeartbeatRequestMessage m:
					OnHeartbeatRequest( session, m );
					break;

				case CommandFrame m:
					OnCommandFrame( session, m );
					break;

				default:
					throw new NotSupportedException( message.ToString() );
			}
		}

		void OnLoginRequest(Session session, LoginRequestMessage req)
		{
			session.OnLoginSucceeded( req.UserId );
			Simulator.Context.Log( "OnLoginRequest {0}, {1}", session, req );

			var res = new LoginResponseMessage()
			{
				Ok = true,
				ClientTime = req.ClientTime,
				ServerTime = (uint)Math.Round( Simulator.SynchronizedClock.LocalTime * 1000 )
			};
			session.Stream.Send( res );

			var f = EnsureCommandFrame();
			var c = f.AddCommand<CreateEntityCommand>();
			c.Settings = new PlayerSettings()
			{
				UserId = req.UserId
			};
		}

		void OnHeartbeatRequest(Session session, HeartbeatRequestMessage req)
		{
			var res = new HeartbeatResponseMessage()
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
						fullSyncFrame = Simulator.ServerTickScheduler.FetchFullSyncFrame2();

					session.Stream.Send( fullSyncFrame );
					session.OnFullSynchornized();
				}
				else if( session.UserId > 0 )
				{
					if( deltaSyncFrame == null )
						deltaSyncFrame = Simulator.ServerTickScheduler.FetchDeltaSyncFrame2();

					session.Stream.Send( deltaSyncFrame );
				}
			}

			fullSyncFrame?.Release();
			deltaSyncFrame?.Release();
		}
	}
}
