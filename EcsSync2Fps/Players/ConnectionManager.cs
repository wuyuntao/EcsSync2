using ProtoBuf;
using System;

namespace EcsSync2.Fps
{
	[ProtoContract]
	[ProtoSubType( typeof( ComponentSnapshot ), 4 )]
	public class ConnectingSnapshot : ComponentSnapshot
	{
		protected override void OnReset()
		{
			ComponentId = 0;
		}

		public override ComponentSnapshot Clone()
		{
			var s = this.Allocate<ConnectingSnapshot>();
			s.ComponentId = ComponentId;
			return s;
		}
	}

	[ProtoContract]
	[ProtoSubType( typeof( ComponentSnapshot ), 5 )]
	public class ConnectedSnapshot : ComponentSnapshot
	{
		protected override void OnReset()
		{
			ComponentId = 0;
		}

		public override ComponentSnapshot Clone()
		{
			var s = this.Allocate<ConnectedSnapshot>();
			s.ComponentId = ComponentId;
			return s;
		}
	}

	[ProtoContract]
	[ProtoSubType( typeof( ComponentSnapshot ), 6 )]
	public class DisconnectedSnapshot : ComponentSnapshot
	{
		protected override void OnReset()
		{
			ComponentId = 0;
		}

		public override ComponentSnapshot Clone()
		{
			var s = this.Allocate<DisconnectedSnapshot>();
			s.ComponentId = ComponentId;
			return s;
		}
	}

	[ProtoContract]
	[ProtoSubType( typeof( ComponentCommand ), 1 )]
	public class PlayerConnectCommand : ComponentCommand
	{
		protected override void OnReset()
		{
			ComponentId = 0;
		}
	}

	[ProtoContract]
	[ProtoSubType( typeof( ComponentEvent ), 2 )]
	public class PlayerConnectedEvent : ComponentEvent
	{
		protected override void OnReset()
		{
			ComponentId = 0;
		}
	}

	public class ConnectionManager : Component
	{
		public Player Player => (Player)Entity;

		protected override void OnCommandReceived(ComponentCommand command)
		{
			if( !Entity.SceneManager.Simulator.IsServer )
				return;

			switch( State )
			{
				case ConnectingSnapshot s1:
					{
						switch( command )
						{
							case PlayerConnectCommand c:
								var e = CreateEvent<PlayerConnectedEvent>();
								ApplyEvent( e );

								Entity.SceneManager.Scene.ApplyEntityCreatedEvent( new CharacterSettings()
								{
									UserId = Player.TheSettings.UserId,
								} );
								return;
						}
					}
					break;
			}

			Entity.SceneManager.Simulator.Context.LogWarning( "Not supported command {0}", command );
		}

		protected override ComponentSnapshot OnEventApplied(ComponentEvent @event)
		{
			switch( State )
			{
				case ConnectingSnapshot s1:
					{
						switch( @event )
						{
							case PlayerConnectedEvent e:
								return CreateSnapshot<ConnectedSnapshot>();
						}
					}
					break;
			}

			throw new NotSupportedException( @event.ToString() );
		}

		protected override void OnFixedUpdate()
		{
		}

		protected override void OnSnapshotRecovered(ComponentSnapshot state)
		{
		}

		protected override ComponentSnapshot CreateSnapshot()
		{
			return CreateSnapshot<ConnectingSnapshot>();
		}

		protected override void OnInitialize()
		{
		}

		protected override void OnStart()
		{
		}

		protected override void OnDestroy()
		{
		}
	}
}
