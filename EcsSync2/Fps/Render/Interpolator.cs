using System;

namespace EcsSync2.Fps
{
	public class Interpolator : Renderer2
	{
		public interface IContext
		{
			void SetPosition(Vector2D vector);
		}

		public IContext Context { get; set; }

		Transform m_transform;

		protected override void OnInitialize()
		{
			base.OnInitialize();

			m_transform = (Transform)Entity.Components.Find( c => c is Transform );
		}

		protected override void OnUpdate()
		{
			Context?.SetPosition( m_transform.Position );
		}

		protected override void OnCommandReceived(ComponentCommand command)
		{
			throw new NotSupportedException( command.ToString() );
		}

		protected override void OnDestroy()
		{
		}

		protected override ComponentSnapshot OnEventApplied(ComponentEvent @event)
		{
			throw new NotSupportedException( @event.ToString() );
		}

		protected override void OnFixedUpdate()
		{
		}

		protected override void OnSnapshotRecovered(ComponentSnapshot state)
		{
		}

		protected override void OnStart()
		{
		}

		protected internal override ComponentSnapshot CreateSnapshot()
		{
			return null;
		}
	}
}
