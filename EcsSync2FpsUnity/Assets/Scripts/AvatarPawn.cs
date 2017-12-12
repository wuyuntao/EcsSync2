using UnityEngine;
using URandom = UnityEngine.Random;

namespace EcsSync2.FpsUnity
{
	public class AvatarPawn : MonoBehaviour
	{
		public MeshRenderer Head;
		public MeshRenderer Body;

		public void Randomnize(int seed)
		{
			URandom.InitState( seed );
			Head.material.color = URandom.ColorHSV();
			Body.material.color = URandom.ColorHSV();
		}
	}
}