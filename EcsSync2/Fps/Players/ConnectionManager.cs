using MessagePack;
using System;

namespace EcsSync2.Fps
{
	[MessagePackObject]
	public class ConnectingSnapshot : ComponentSnapshot, IComponentSnapshotUnion
	{
		public override Snapshot Clone()
		{
			return this.Allocate<ConnectingSnapshot>();
		}
	}

	[MessagePackObject]
	public class ConnectedSnapshot : ComponentSnapshot, IComponentSnapshotUnion
	{
		public override Snapshot Clone()
		{
			return this.Allocate<ConnectedSnapshot>();
		}
	}

	[MessagePackObject]
	public class DisconnectedSnapshot : ComponentSnapshot, IComponentSnapshotUnion
	{
		public override Snapshot Clone()
		{
			return this.Allocate<DisconnectedSnapshot>();
		}
	}

	[MessagePackObject]
	public class PlayerConnectCommand : ComponentCommand
	{
	}

	[MessagePackObject]
	public class PlayerConnectedEvent : ComponentEvent, IEvent
	{
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
								var e = AllocateEvent<PlayerConnectedEvent>();
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

			throw new NotSupportedException( command.ToString() );
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
								return e.Allocate<ConnectedSnapshot>();
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
			return Entity.SceneManager.Simulator.ReferencableAllocator.Allocate<ConnectingSnapshot>();
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
