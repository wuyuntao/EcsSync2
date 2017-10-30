﻿using System;

namespace EcsSync2.FpsExample
{
	class CharacterMotionControllerSnapshot : Snapshot
	{
		public Vector2D InputDirection;

		public float InputMagnitude;

		public float MaxSpeed;

		public override Snapshot Clone()
		{
			var s = this.Allocate<CharacterMotionControllerSnapshot>();
			s.InputDirection = InputDirection;
			s.InputMagnitude = InputMagnitude;
			s.MaxSpeed = MaxSpeed;
			return s;
		}
	}

	public class MoveCharacterCommand : ComponentCommand
	{
		public Vector2D InputDirection;

		public float InputMagnitude;
	}

	public class InputChangedEvent : Event
	{
		public Vector2D InputDirection;

		public float InputMagnitude;
	}

	public class CharacterMotionController : Component
	{
		protected override void OnCommandReceived(Command command)
		{
			switch( command )
			{
				case MoveCharacterCommand c:
					var e = c.Allocate<InputChangedEvent>();
					var s = (CharacterMotionControllerSnapshot)State;
					e.InputDirection = c.InputMagnitude > 0 ? c.InputDirection : s.InputDirection;
					e.InputMagnitude = c.InputMagnitude;
					ApplyEvent( e );
					break;

				default:
					throw new NotSupportedException( command.ToString() );
			}
		}

		protected override Snapshot OnEventApplied(Event @event)
		{
			switch( @event )
			{
				case InputChangedEvent e:
					var s = (CharacterMotionControllerSnapshot)State.Clone();
					s.InputDirection = e.InputDirection;
					s.InputMagnitude = e.InputMagnitude;
					return s;

				default:
					throw new NotSupportedException( @event.ToString() );
			}
		}

		protected override void OnFixedUpdate()
		{
			var s = (CharacterMotionControllerSnapshot)State;
			var velocity = s.InputDirection * s.InputMagnitude * s.MaxSpeed;
			var offset = velocity * Configuration.SimulationDeltaTime / 1000f;

			Character.Transform.ApplyTransformVelocityChangedEvent( velocity );

			if( offset.LengthSquared() > 0 )
				Character.Transform.ApplyTransformMovedEvent( offset );
		}

		protected override void OnSnapshotRecovered(Snapshot state)
		{
		}

		protected override Snapshot CreateSnapshot()
		{
			var s = Entity.SceneManager.Simulator.ReferencableAllocator.Allocate<CharacterMotionControllerSnapshot>();
			s.InputDirection = new Vector2D( 0, 1 );
			s.InputMagnitude = 0;
			s.MaxSpeed = 3;
			return s;
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

		Character Character => (Character)Entity;
	}
}