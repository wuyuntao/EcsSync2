using System;

namespace EcsSync2.FpsExample
{
	public abstract class ConnectionManagerSnapshot : ComponentSnapshot
	{
	}

	public class ConnectingSnapshot : ConnectionManagerSnapshot
	{
		public override Snapshot Clone()
		{
			return this.Allocate<ConnectingSnapshot>();
		}
	}

	public class ConnectedSnapshot : ConnectionManagerSnapshot
	{
		public override Snapshot Clone()
		{
			return this.Allocate<ConnectedSnapshot>();
		}
	}

	public class DisconnectedSnapshot : ConnectionManagerSnapshot
	{
		public override Snapshot Clone()
		{
			return this.Allocate<DisconnectedSnapshot>();
		}
	}

	public class PlayerConnectCommand : ComponentCommand
	{
	}

	public class PlayerConnectedEvent : ComponentEvent
	{
	}

	public class ConnectionManager : Component
	{
		public Player Player => (Player)Entity;

		protected override void OnCommandReceived(Command command)
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
								var e = c.Allocate<PlayerConnectedEvent>();
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

		protected override Snapshot OnEventApplied(Event @event)
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

		protected override void OnSnapshotRecovered(Snapshot state)
		{
		}

		protected override Snapshot CreateSnapshot()
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
