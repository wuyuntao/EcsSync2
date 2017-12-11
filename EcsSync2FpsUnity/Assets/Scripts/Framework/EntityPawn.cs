using EcsSync2.Fps;
using UnityEngine;
using UTransform = UnityEngine.Transform;

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
	}
}