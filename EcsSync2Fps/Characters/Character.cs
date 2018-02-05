using ProtoBuf;

namespace EcsSync2.Fps
{
	[ProtoContract]
	[ProtoSubType( typeof( EntitySettings ), 1 )]
	public class CharacterSettings : EntitySettings
	{
		[ProtoMember( 1 )]
		public ulong UserId;
	}

	public class Character : Entity
	{
		public Transform Transform { get; private set; }
		public CharacterMotionController MotionController { get; private set; }
		public Jumper Jumper { get; private set; }
		public Interpolator Interpolator { get; private set; }
		public Animator Animator { get; private set; }

		protected override void OnInitialize()
		{
			Transform = AddComponent<Transform>( new TransformSettings() );
			MotionController = AddComponent<CharacterMotionController>();
			Jumper = AddComponent<Jumper>();

			Interpolator = AddComponent<Interpolator>();
			Animator = AddComponent<CharacterAnimator>();
		}

		public CharacterSettings TheSettings => (CharacterSettings)Settings;

		public bool IsLocalCharacter => TheSettings.UserId == SceneManager.Simulator.LocalUserId;

		public override bool IsLocalEntity => SceneManager.Simulator.IsServer || IsLocalCharacter;
	}
}
