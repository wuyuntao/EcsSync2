using EcsSync2.Fps;
using UnityEngine;

public class CharacterPawn : MonoBehaviour
{
	public MeshRenderer Head;
	public MeshRenderer Body;
	public Character Character;

	public void Initialize(Character character)
	{
		Character = character;

		Random.InitState( (int)(uint)character.Id );
		Head.material.color = Random.ColorHSV();
		Body.material.color = Random.ColorHSV();
	}

	void Update()
	{
		//if( Character.Transform.Velocity.LengthSquared() > 0 )
		//{
		//	transform.position = Character.Transform.Position.AsUnity3();
		//}
	}
}
