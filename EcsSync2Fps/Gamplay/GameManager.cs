using ProtoBuf;

namespace EcsSync2.Fps
{
	[ProtoContract]
	[ProtoSubType( typeof( EntitySettings ), 2 )]
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
