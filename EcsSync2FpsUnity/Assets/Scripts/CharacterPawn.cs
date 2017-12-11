using EcsSync2.Fps;
using UnityEngine;
using URandom = UnityEngine.Random;
using UTransform = UnityEngine.Transform;

namespace EcsSync2.FpsUnity
{
	public class CharacterPawn : EntityPawn
	{
		public MeshRenderer Head;
		public MeshRenderer Body;
		public UTransform CameraPod;
		public Character Character;

		public override void Initialize(Entity entity)
		{
			base.Initialize( entity );

			Character = (Character)entity;

			Character.Interpolator.Context = new InterpolatorContext( transform );

			URandom.InitState( (int)(uint)entity.Id );
			Head.material.color = URandom.ColorHSV();
			Body.material.color = URandom.ColorHSV();
		}
	}
}
