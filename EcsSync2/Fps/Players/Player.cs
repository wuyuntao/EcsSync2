using ProtoBuf;

namespace EcsSync2.Fps
{
	[ProtoContract]
	public class PlayerSettings : EntitySettings
	{
		[ProtoMember( 1 )]
		public ulong UserId;

		[ProtoMember( 2 )]
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
