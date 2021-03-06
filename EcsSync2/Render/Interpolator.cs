﻿using System;

namespace EcsSync2
{
	public class Interpolator : Renderer
	{
		public interface IContext
		{
			void SetPosition(Vector2D vector);
		}

		Transform m_transform;
		IContext m_context;

		protected override void OnInitialize()
		{
			base.OnInitialize();

			m_transform = (Transform)Entity.Components.Find( c => c is Transform );
		}

		protected override void OnUpdate()
		{
			m_context?.SetPosition( m_transform.Position );
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

		public IContext Context
		{
			get { return m_context; }
			set
			{
				m_context = value;

				OnUpdate();
			}
		}
	}
}
