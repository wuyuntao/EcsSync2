using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using System;
using System.Diagnostics;

namespace EcsSync2.Fps
{
	public sealed class FpsClient
	{
		public event Action<Simulator> OnLogin;

		public string Address { get; }
		public int Port { get; }
		public string ConnectKey { get; }

		public ulong UserId { get; private set; }

		public NetManager NetManager { get; }
		public NetPeer NetPeer { get; private set; }
		public ILogger Logger { get; }

		Simulator.IContext Context { get; }
		public Simulator Simulator { get; private set; }
		public BattleScene Scene { get; private set; }

		Stopwatch Stopwatch;
		long LastUpdateMs;
		long LastHeartbeatMs;

		public FpsClient(Simulator.IContext context, string address, int port, ulong userId, string connectKey = "EcsSync2")
		{
			Address = address;
			Port = port;
			ConnectKey = connectKey;
			UserId = userId;
			Logger = Context = context;

			var listener = new EventBasedNetListener();
			listener.PeerConnectedEvent += Listener_PeerConnectedEvent;
			listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;
			listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
			listener.NetworkErrorEvent += Listener_NetworkErrorEvent;

			NetManager = new NetManager( listener, 100, ConnectKey )
			{
				MergeEnabled = true,
			};

			NetManager.Start();
			NetManager.Connect( Address, Port );

			Stopwatch = Stopwatch.StartNew();
		}

		public void Update()
		{
			NetManager.PollEvents();

			if( Simulator != null )
			{
				var deltaMs = Stopwatch.ElapsedMilliseconds - LastUpdateMs;
				LastUpdateMs = Stopwatch.ElapsedMilliseconds;

				Simulator.Simulate( deltaMs / 1000f );

				var commandFrame = Simulator.ClientTickScheduler.FetchCommandFrame();
				while( commandFrame != null )
				{
					var msg = CommandFrameMessage.FromCommandFrame( UserId, commandFrame );
					var env = new MessageEnvelop() { Message = msg };
					var bytes = MessagePackSerializer.Serialize( env );

					commandFrame.Release();
					commandFrame = Simulator.ClientTickScheduler.FetchCommandFrame();

					NetPeer.Send( bytes, SendOptions.ReliableOrdered );
				}

				if( Stopwatch.ElapsedMilliseconds - LastHeartbeatMs > Configuration.HeartbeatIntervalTime )
				{
					LastHeartbeatMs = Stopwatch.ElapsedMilliseconds;

					var msg = new HeartbeatRequestMessage() { ClientTime = (uint)LastHeartbeatMs };
					var env = new MessageEnvelop() { Message = msg };
					var bytes = MessagePackSerializer.Serialize( env );

					NetPeer.Send( bytes, SendOptions.ReliableOrdered );
				}
			}
		}

		public void Stop()
		{
			NetManager.Stop();
		}

		void Listener_PeerConnectedEvent(NetPeer peer)
		{
			Logger?.Log( "Listener_PeerConnectedEvent {0}", peer );
			NetPeer = peer;

			var req = new LoginRequestMessage() { UserId = UserId, ClientTime = (uint)Stopwatch.ElapsedMilliseconds };
			var env = new MessageEnvelop() { Message = req };
			var bytes = MessagePackSerializer.Serialize( env );
			peer.Send( bytes, SendOptions.ReliableOrdered );
		}

		void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			Logger?.Log( "Listener_PeerDisconnectedEvent {0}, {1}", peer, disconnectInfo );
		}

		void Listener_NetworkReceiveEvent(NetPeer peer, NetDataReader reader)
		{
			var me = MessagePackSerializer.Deserialize<MessageEnvelop>( reader.Data );

			switch( me.Message )
			{
				case LoginResponseMessage m:
					LastUpdateMs = Stopwatch.ElapsedMilliseconds;
					Simulator = new Simulator( Context, false, true, null, UserId );
					OnLogin?.Invoke( Simulator );
					Simulator.SynchronizedClock.Synchronize( m.ServerTime / 1000f, ( LastUpdateMs - m.ClientTime ) / 1000f );
					Scene = Simulator.SceneManager.LoadScene<BattleScene>();
					Logger?.Log( "Login {0}", peer );
					break;

				case HeartbeatResponseMessage m:
					Simulator.SynchronizedClock.Synchronize( m.ServerTime / 1000f, ( Stopwatch.ElapsedMilliseconds - m.ClientTime ) / 1000f );
					//Logger?.Log( "Heartbeat {0}", peer );
					break;

				case FullSyncFrameMessage m:
					var f1 = m.ToFullSyncFrame( Simulator );
					Simulator.ClientTickScheduler.ReceiveSyncFrame( f1 );
					Logger?.Log( "FullSyncFrame {0}", peer );
					break;

				case DeltaSyncFrameMessage m:
					var f2 = m.ToDeltaSyncFrame( Simulator );
					Simulator.ClientTickScheduler.ReceiveSyncFrame( f2 );
					//Logger?.Log( "DeltaSyncFrame {0}", peer );
					break;

				default:
					throw new NotSupportedException( me.Message.ToString() );
			}
		}

		void Listener_NetworkErrorEvent(NetEndPoint endPoint, int socketErrorCode)
		{
			Logger?.LogError( "Listener_NetworkErrorEvent {0}, {1}", endPoint, socketErrorCode );
		}
	}
}
