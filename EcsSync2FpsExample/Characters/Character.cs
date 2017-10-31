using MessagePack;

namespace EcsSync2.FpsExample
{
	[MessagePackObject]
	public class CharacterSettings : EntitySettings, IEntitySettingsUnion
	{
		[Key( 0 )]
		public ulong UserId;
	}

	public class Character : Entity
	{
		public Transform Transform { get; private set; }
		public CharacterMotionController MotionController { get; private set; }

		protected override void OnInitialize()
		{
			Transform = AddComponent<Transform>( new TransformSettings() );
			MotionController = AddComponent<CharacterMotionController>();
		}
	}
}
