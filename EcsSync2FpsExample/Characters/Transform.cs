using System;

namespace EcsSync2.FpsExample
{
	public class TransformSettings : ComponentSettings
	{
		public Vector2D Position;

		public Vector2D Velocity;

		public bool IsStatic;
	}

	public class TransformSnapshot : Snapshot
	{
		public Vector2D Position;

		public Vector2D Velocity;

		public override Snapshot Clone()
		{
			var s = this.Allocate<TransformSnapshot>();
			s.Position = Position;
			s.Velocity = Velocity;
			return s;
		}
	}

	public class TransformMovedEvent : Event
	{
		public Vector2D Position;
	}

	public class TransformVelocityChangedEvent : Event
	{
		public Vector2D Velocity;
	}

	public class Transform : Component
	{
		protected override void OnCommandReceived(Command command)
		{
			throw new NotSupportedException( command.ToString() );
		}

		protected override Snapshot OnEventApplied(Event @event)
		{
			switch( @event )
			{
				case TransformMovedEvent e:
					var s1 = (TransformSnapshot)State.Clone();
					s1.Position = e.Position;
					return s1;

				case TransformVelocityChangedEvent e:
					var s2 = (TransformSnapshot)State.Clone();
					s2.Velocity = e.Velocity;
					return s2;

				default:
					throw new NotSupportedException( @event.ToString() );
			}
		}

		protected override void OnFixedUpdate()
		{
		}

		protected override void OnSnapshotRecovered(Snapshot state)
		{
		}

		protected override Snapshot CreateSnapshot()
		{
			return Entity.SceneManager.Simulator.ReferencableAllocator.Allocate<TransformSnapshot>();
		}

		internal void ApplyTransformMovedEvent(Vector2D offset)
		{
			var s = (TransformSnapshot)State;
			var e = s.Allocate<TransformMovedEvent>();
			e.Position = s.Position + offset;
			ApplyEvent( e );
		}

		internal void ApplyTransformVelocityChangedEvent(Vector2D velocity)
		{
			var s = (TransformSnapshot)State;
			if( s.Velocity == velocity )
				return;

			var e = State.Allocate<TransformVelocityChangedEvent>();
			e.Velocity = velocity;
			ApplyEvent( e );
		}

		protected override void OnInitialize()
		{
		}

		protected override void OnStart()
		{
		}

		protected override void OnDestroy()
		{
		}
	}
}
