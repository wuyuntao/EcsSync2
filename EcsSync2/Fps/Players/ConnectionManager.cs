using MessagePack;
using System;

namespace EcsSync2.Fps
{
	[MessagePackObject]
	public class ConnectingSnapshot : ComponentSnapshot, IComponentSnapshot
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

	[MessagePackObject]
	public class ConnectedSnapshot : ComponentSnapshot, IComponentSnapshot
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

	[MessagePackObject]
	public class DisconnectedSnapshot : ComponentSnapshot, IComponentSnapshot
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

	[MessagePackObject]
	public class PlayerConnectCommand : ComponentCommand
	{
		protected override void OnReset()
		{
			ComponentId = 0;
		}
	}

	[MessagePackObject]
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

		protected internal override ComponentSnapshot CreateSnapshot()
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
