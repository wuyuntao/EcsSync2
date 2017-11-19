using EcsSync2.Fps;
using UnityEngine;
using URandom = UnityEngine.Random;

namespace EcsSync2.FpsUnity
{
    public class CharacterPawn : MonoBehaviour
    {
        public MeshRenderer Head;
        public MeshRenderer Body;
        public Character Character;

        public void Initialize(Character character)
        {
            Character = character;
            Character.Transform.OnMoved += OnMoved;

            URandom.InitState((int)(uint)character.Id);
            Head.material.color = URandom.ColorHSV();
            Body.material.color = URandom.ColorHSV();
        }

        void OnMoved()
        {
            transform.position = Character.Transform.Position.ToUnityPos();
        }

        void Update()
        {
            //if( Character.Transform.Velocity.LengthSquared() > 0 )
            //{
            //	transform.position = Character.Transform.Position.AsUnity3();
            //}
        }
    }
}
