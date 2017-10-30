namespace EcsSync2.FpsExample
{
	public class GameManagerSettings : EntitySettings
	{
		protected override EntitySettings Clone()
		{
			throw new System.NotImplementedException();
		}
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
