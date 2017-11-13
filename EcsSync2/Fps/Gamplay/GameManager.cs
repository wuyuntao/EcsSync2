using MessagePack;

namespace EcsSync2.Fps
{
	[MessagePackObject]
	public class GameManagerSettings : IEntitySettings
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
