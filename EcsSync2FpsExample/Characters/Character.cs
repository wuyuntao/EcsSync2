namespace EcsSync2.FpsExample
{
	public class CharacterSettings : EntitySettings
	{
		protected override EntitySettings Clone()
		{
			throw new System.NotImplementedException();
		}
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
