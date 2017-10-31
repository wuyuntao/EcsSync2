using MessagePack;

namespace EcsSync2.FpsExample
{
	[MessagePackObject]
	public class GameManagerSettings : EntitySettings, IEntitySettingsUnion
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
