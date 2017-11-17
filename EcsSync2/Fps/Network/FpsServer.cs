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
		CommandFrame m_commandFrame;

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

			if( m_commandFrame != null )
			{
				m_commandFrame.Time = Simulator.FixedTime + Configuration.SimulationDeltaTime;

				Simulator.CommandQueue.EnqueueCommands( 0, m_commandFrame );

				m_commandFrame.Release();
				m_commandFrame = null;
			}

			Simulator.Simulate( deltaMs / 1000f );

			if( Peers.Count > 0 )
			{
				var deltaSyncFrame = Simulator.ServerTickScheduler.FetchDeltaSyncFrame();
				while( deltaSyncFrame != null )
				{
					//Logger?.Log( "Send deltaSyncFrame {0}", deltaSyncFrame.Time );

					var msg = DeltaSyncFrameMessage.FromDeltaSyncFrame( deltaSyncFrame );
					var env = new MessageEnvelop() { Message = msg };
					var bytes = MessagePackSerializer.Serialize( env );
					deltaSyncFrame.Release();

					foreach( var p in Peers )
						p.Send( bytes, SendOptions.ReliableOrdered );

					deltaSyncFrame = Simulator.ServerTickScheduler.FetchDeltaSyncFrame();
				}
			}

			if( NewPeers.Count > 0 )
			{
				var fullSyncFrame = Simulator.ServerTickScheduler.FetchFullSyncFrame();

				Logger?.Log( "Send fullSyncFrame {0}", fullSyncFrame.Time );

				var msg = FullSyncFrameMessage.FromFullSyncFrame( fullSyncFrame );
				var env = new MessageEnvelop() { Message = msg };
				var bytes = MessagePackSerializer.Serialize( env );
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

			if( Peers.Remove( peer ) || NewPeers.Remove( peer ) )
				Logger?.Log( "Listener_PeerDisconnectedEvent {0} removed", peer );
		}

		void Listener_NetworkReceiveEvent(NetPeer peer, NetDataReader reader)
		{
			var me = MessagePackSerializer.Deserialize<MessageEnvelop>( reader.Data );

			switch( me.Message )
			{
				case LoginRequestMessage m:
					NewPeers.Add( peer );

					var res1 = new LoginResponseMessage() { Ok = true, ClientTime = m.ClientTime, ServerTime = (uint)Stopwatch.ElapsedMilliseconds };
					var enb1 = new MessageEnvelop() { Message = res1 };
					peer.Send( MessagePackSerializer.Serialize( enb1 ), SendOptions.ReliableOrdered );
					Logger?.Log( "Login {0}", peer );

					EnsureCommandFrame();
					var c = m_commandFrame.AddCommand<CreateEntityCommand>();
					c.Settings = new PlayerSettings() { UserId = m.UserId };

					break;

				case HeartbeatRequestMessage m:
					var res2 = new HeartbeatResponseMessage() { ClientTime = m.ClientTime, ServerTime = (uint)Stopwatch.ElapsedMilliseconds };
					var env2 = new MessageEnvelop() { Message = res2 };
					peer.Send( MessagePackSerializer.Serialize( env2 ), SendOptions.ReliableOrdered );
					//Logger?.Log( "Heartbeat {0}", peer );
					break;

				case CommandFrameMessage m:
					var f = m.ToCommandFrame( Simulator );
					Simulator.CommandQueue.EnqueueCommands( m.UserId, f );
					//Logger?.Log( "CommandFrame {0}", peer );
					break;

				default:
					throw new NotSupportedException( me.Message.ToString() );
			}
		}

		void Listener_NetworkErrorEvent(NetEndPoint endPoint, int socketErrorCode)
		{
			Logger?.LogError( "Listener_NetworkErrorEvent {0}, {1}", endPoint, socketErrorCode );
		}

		void EnsureCommandFrame()
		{
			if( m_commandFrame == null )
			{
				m_commandFrame = Simulator.ReferencableAllocator.Allocate<CommandFrame>();
				m_commandFrame.Retain();
			}
		}
	}
}
