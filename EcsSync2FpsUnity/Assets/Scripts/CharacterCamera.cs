using UnityEngine;
using UTransform = UnityEngine.Transform;

namespace EcsSync2.FpsUnity
{
	public class CharacterCamera : MonoBehaviour
	{
		[SerializeField]
		float m_smoothTime = 0.15f;

		UTransform m_target;
		Vector3 m_lastPostion;
		Vector3 m_currentVelocity;

		void LateUpdate()
		{
			if( m_target != null )
			{
				if( m_smoothTime > 0 )
				{
					m_lastPostion = transform.position = Vector3.SmoothDamp( m_lastPostion,
						m_target.position,
						ref m_currentVelocity,
						m_smoothTime,
						float.MaxValue,
						Time.deltaTime );
				}
				else
				{
					m_lastPostion = transform.position = m_target.position;
				}
			}
		}

		public UTransform FollowTarget
		{
			get { return m_target; }
			set
			{
				m_target = value;
				m_lastPostion = transform.position = m_target.position;
				transform.localRotation = m_target.rotation;
			}
		}
	}
}