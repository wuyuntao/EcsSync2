using EcsSync2.Fps;
using UnityEngine;
using UTransform = UnityEngine.Transform;
using UAnimator = UnityEngine.Animator;

namespace EcsSync2.FpsUnity
{
	public abstract class EntityPawn : MonoBehaviour, Entity.IContext
	{
		Entity m_entity;

		public virtual void Initialize(Entity entity)
		{
			m_entity = entity;
			m_entity.Context = this;
		}

		protected class InterpolatorContext : Interpolator.IContext
		{
			UTransform m_transform;

			public InterpolatorContext(UTransform transform)
			{
				m_transform = transform;
			}

			void Interpolator.IContext.SetPosition(Vector2D position)
			{
				m_transform.localPosition = position.ToUnityPos();
			}
		}

		protected class AnimatorContext : Fps.Animator.IContext
		{
			UAnimator m_animator;
			string m_lastState;
			uint? m_lastStateContext;

			public AnimatorContext(UAnimator animator)
			{
				m_animator = animator;
			}

			public void SetBool(string name, bool value)
			{
				m_animator.SetBool( name, value );
			}

			public void SetFloat(string name, float value)
			{
				m_animator.SetFloat( name, value );
			}

			public void SetInt(string name, int value)
			{
				m_animator.SetInteger( name, value );
			}

			public void SetState(string name, uint? context)
			{
				if( m_lastState != name || m_lastStateContext != context )
				{
					m_animator.SetTrigger( name );
					m_lastState = name;
					m_lastStateContext = context;
				}
			}
		}
	}
}