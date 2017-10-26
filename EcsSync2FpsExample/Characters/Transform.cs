using System;

namespace EcsSync2.FpsExample
{
	public class TransformSettings : ComponentSettings
	{
		public Vector2D Position;

		public float RotationAngle;

		public bool IsStatic;
	}

	public class TransformMovedEvent : Event
	{
		public Vector2D Position;

		public float RotationAngle;
	}


	public class Transform : Component
	{
		protected override void OnCommandReceived(Command command)
		{
			throw new NotImplementedException();
		}

		protected override Snapshot OnEventApplied(Snapshot state, Event @event)
		{
			throw new NotImplementedException();
		}

		protected override void OnFixedUpdate()
		{
			throw new NotImplementedException();
		}

		protected override void OnSnapshotRecovered(Snapshot state)
		{
			throw new NotImplementedException();
		}

		protected override Snapshot OnStart()
		{
			throw new NotImplementedException();
		}
	}
}
