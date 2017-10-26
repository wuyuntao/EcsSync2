namespace EcsSync2
{
	public class PlayerSettings : EntitySettings
	{
	}

	public class Player : Entity
	{
		public ulong UserId;

		public bool IsAI;

		protected override void OnInitialize()
		{
		}
	}
}
