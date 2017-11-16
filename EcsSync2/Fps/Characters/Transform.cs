using MessagePack;
using System;

namespace EcsSync2.Fps
{
	public class TransformSettings : ComponentSettings
	{
		public Vector2D Position;

		public Vector2D Velocity;

		public bool IsStatic;
	}

	[MessagePackObject]
	public class TransformSnapshot : ComponentSnapshot, IComponentSnapshotUnion
	{
		[Key( 20 )]
		public Vector2D Position;

		[Key( 21 )]
		public Vector2D Velocity;

		public override Snapshot Clone()
		{
			var s = this.Allocate<TransformSnapshot>();
			s.Position = Position;
			s.Velocity = Velocity;
			return s;
		}
	}

	[MessagePackObject]
	public class TransformMovedEvent : ComponentEvent, IEvent
	{
		[Key( 20 )]
		public Vector2D Position;
	}

	[MessagePackObject]
	public class TransformVelocityChangedEvent : ComponentEvent, IEvent
	{
		[Key( 20 )]
		public Vector2D Velocity;
	}

	public class Transform : Component
	{
		public Action<Transform> OnMoved;

		protected override void OnCommandReceived(ComponentCommand command)
		{
			throw new NotSupportedException( command.ToString() );
		}

		protected override ComponentSnapshot OnEventApplied(ComponentEvent @event)
		{
			switch( @event )
			{
				case TransformMovedEvent e:
					var s1 = (TransformSnapshot)State.Clone();
					s1.Position = e.Position;
					OnMoved?.Invoke( this );
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

		protected override void OnSnapshotRecovered(ComponentSnapshot state)
		{
		}

		protected internal override ComponentSnapshot CreateSnapshot()
		{
			return Entity.SceneManager.Simulator.ReferencableAllocator.Allocate<TransformSnapshot>();
		}

		internal void ApplyTransformMovedEvent(Vector2D offset)
		{
			var s = (TransformSnapshot)State;
			var e = AllocateEvent<TransformMovedEvent>();
			e.Position = s.Position + offset;
			ApplyEvent( e );
		}

		internal void ApplyTransformVelocityChangedEvent(Vector2D velocity)
		{
			var s = (TransformSnapshot)State;
			if( s.Velocity == velocity )
				return;

			var e = AllocateEvent<TransformVelocityChangedEvent>();
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

		TransformSnapshot TheState => (TransformSnapshot)State;

		public Vector2D Position => TheState.Position;

		public Vector2D Velocity => TheState.Velocity;
	}
}
