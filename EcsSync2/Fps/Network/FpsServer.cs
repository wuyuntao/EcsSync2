using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EcsSync2.Fps
{
	public sealed class FpsServer
	{
		public string Address { get; }
		public int Port { get; }
		public string ConnectKey { get; }

		public NetManager NetManager { get; }
		public ILogger Logger { get; }

		public Simulator Simulator { get; }
		public BattleScene Scene { get; }

		Stopwatch Stopwatch;
		long LastUpdateMs;
		List<NetPeer> Peers = new List<NetPeer>();
		List<NetPeer> NewPeers = new List<NetPeer>();

		public FpsServer(Simulator.IContext context, string address, int port, string connectKey = "EcsSync2")
		{
			Address = address;
			Port = port;
			ConnectKey = connectKey;
			Logger = context;

			var listener = new EventBasedNetListener();
			listener.PeerConnectedEvent += Listener_PeerConnectedEvent;
			listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;
			listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;
			listener.NetworkErrorEvent += Listener_NetworkErrorEvent;

			NetManager = new NetManager( listener, 100, ConnectKey )
			{
				MergeEnabled = true,
			};

			NetManager.Start( Port );

			Simulator = new Simulator( context, true, false, 1, null );
			Scene = Simulator.SceneManager.LoadScene<BattleScene>();
			Stopwatch = Stopwatch.StartNew();
		}

		public void Update()
		{
			NetManager.PollEvents();

			var deltaMs = Stopwatch.ElapsedMilliseconds - LastUpdateMs;
			LastUpdateMs = Stopwatch.ElapsedMilliseconds;

			Simulator.Simulate( deltaMs / 1000f );

			if( Peers.Count > 0 )
			{
				var deltaSyncFrame = Simulator.ServerTickScheduler.FetchDeltaSyncFrame();
				while( deltaSyncFrame != null )
				{
					Logger.Log( "Send deltaSyncFrame {0}", deltaSyncFrame.Time );

					var bytes = MessagePackSerializer.Serialize( DeltaSyncFrameMessage.FromDeltaSyncFrame( deltaSyncFrame ) );
					deltaSyncFrame.Release();

					foreach( var p in NewPeers )
						p.Send( bytes, SendOptions.ReliableOrdered );

					deltaSyncFrame = Simulator.ServerTickScheduler.FetchDeltaSyncFrame();
				}
			}

			if( NewPeers.Count > 0 )
			{
				var fullSyncFrame = Simulator.ServerTickScheduler.FetchFullSyncFrame();

				Logger.Log( "Send deltaSyncFrame {0}", fullSyncFrame.Time );

				var bytes = MessagePackSerializer.Serialize( FullSyncFrameMessage.FromFullSyncFrame( fullSyncFrame ) );
				fullSyncFrame.Release();

				foreach( var p in NewPeers )
					p.Send( bytes, SendOptions.ReliableOrdered );

				Peers.AddRange( NewPeers );
				NewPeers.Clear();
			}
		}

		public void Stop()
		{
			NetManager.Stop();
		}

		void Listener_PeerConnectedEvent(NetPeer peer)
		{
			Logger?.Log( "Listener_PeerConnectedEvent {0}", peer );
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
				case LoginRequestMessage m:
					NewPeers.Add( peer );

					var res1 = new LoginResponseMessage() { Ok = true };
					peer.Send( MessagePackSerializer.Serialize( res1 ), SendOptions.ReliableOrdered );
					break;

				case HeartbeatRequestMessage m:
					var res2 = new HeartbeatResponseMessage() { ClientTime = m.ClientTime, ServerTime = (uint)Stopwatch.ElapsedMilliseconds };
					peer.Send( MessagePackSerializer.Serialize( res2 ), SendOptions.ReliableOrdered );
					break;

				case CommandFrameMessage m:
					var f = m.ToCommandFrame( Simulator );
					Simulator.CommandQueue.EnqueueCommands( m.UserId, f );
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
