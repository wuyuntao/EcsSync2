namespace EcsSync2.FpsExample
{
	public class PlayerSettings : EntitySettings
	{
		public ulong UserId;

		public bool IsAI;

		protected override EntitySettings Clone()
		{
			throw new System.NotImplementedException();
		}
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
