using EcsSync2.Fps;
using UTransform = UnityEngine.Transform;

namespace EcsSync2.FpsUnity
{
	public class CharacterPawn : EntityPawn
	{
		public UTransform AvatarRoot;
		public AvatarPawn AvatarPawnPrefab;
		public UTransform CameraPod;

		public Character Character;
		public AvatarPawn Avatar;

		public override void Initialize(Entity entity)
		{
			base.Initialize( entity );

			Character = (Character)entity;
			Character.Interpolator.Context = new InterpolatorContext( transform );

			Avatar = Instantiate( AvatarPawnPrefab, AvatarRoot ).GetComponent<AvatarPawn>();
			Avatar.Randomnize( (int)(uint)entity.Id );
			Character.Animator.Context = new AnimatorContext( Avatar.Animator );
		}
	}
}
