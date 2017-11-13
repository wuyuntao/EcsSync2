using MessagePack;

namespace EcsSync2.Fps
{
	[MessagePackObject]
	public class PlayerSettings : IEntitySettings
	{
		[Key( 0 )]
		public ulong UserId;

		[Key( 1 )]
		public bool IsAI;
	}

	public class Player : Entity
	{
		public ConnectionManager ConnectionManager { get; private set; }

		protected override void OnInitialize()
		{
			ConnectionManager = AddComponent<ConnectionManager>();
		}

		public PlayerSettings TheSettings => (PlayerSettings)Settings;
	}
}
