namespace EcsSync2
{
	public class GameManagerSettings : EntitySettings
	{
	}

	public class GameManager : Entity
	{
		public ProcessController ProcessController { get; private set; }

		protected override void OnInitialize()
		{
			ProcessController = AddComponent<ProcessController>();
		}
	}
}
