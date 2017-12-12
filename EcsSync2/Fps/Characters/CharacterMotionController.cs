using ProtoBuf;
using System;

namespace EcsSync2.Fps
{
	[ProtoContract]
	class CharacterMotionControllerSnapshot : ComponentSnapshot
	{
		[ProtoMember( 21 )]
		public Vector2D InputDirection;

		[ProtoMember( 22 )]
		public float InputMagnitude;

		[ProtoMember( 23 )]
		public float MaxSpeed;

		protected override void OnReset()
		{
			ComponentId = 0;
			InputDirection = Vector2D.Zero;
			InputMagnitude = 0;
			MaxSpeed = 0;
		}

		public override ComponentSnapshot Clone()
		{
			var s = this.Allocate<CharacterMotionControllerSnapshot>();
			s.ComponentId = ComponentId;
			s.InputDirection = InputDirection;
			s.InputMagnitude = InputMagnitude;
			s.MaxSpeed = MaxSpeed;
			return s;
		}

		protected internal override bool IsApproximate(ComponentSnapshot other)
		{
			if( !( other is CharacterMotionControllerSnapshot s ) )
				return false;

			return
				IsApproximate( ComponentId, s.ComponentId ) &&
				IsApproximate( InputDirection, s.InputDirection ) &&
				IsApproximate( InputMagnitude, s.InputMagnitude ) &&
				IsApproximate( MaxSpeed, s.MaxSpeed );
		}
	}

	[ProtoContract]
	public class MoveCharacterCommand : ComponentCommand
	{
		[ProtoMember( 21 )]
		public Vector2D InputDirection;

		[ProtoMember( 22 )]
		public float InputMagnitude;

		protected override void OnReset()
		{
			ComponentId = 0;
			InputDirection = Vector2D.Zero;
			InputMagnitude = 0;
		}
	}

	[ProtoContract]
	public class InputChangedEvent : ComponentEvent
	{
		[ProtoMember( 21 )]
		public Vector2D InputDirection;

		[ProtoMember( 22 )]
		public float InputMagnitude;

		protected override void OnReset()
		{
			ComponentId = 0;
			InputDirection = Vector2D.Zero;
			InputMagnitude = 0;
		}
	}

	public class CharacterMotionController : Component
	{
		protected override void OnCommandReceived(ComponentCommand command)
		{
			switch( command )
			{
				case MoveCharacterCommand c:
					if( Entity.SceneManager.Simulator.IsServer || ( Entity is Character character && character.TheSettings.UserId == Entity.SceneManager.Simulator.LocalUserId ) )
					{
						var e = CreateEvent<InputChangedEvent>();
						var s = (CharacterMotionControllerSnapshot)State;
						e.InputDirection = c.InputMagnitude > 0 ? c.InputDirection : s.InputDirection;
						e.InputMagnitude = c.InputMagnitude;
						ApplyEvent( e );
					}
					break;

				default:
					throw new NotSupportedException( command.ToString() );
			}
		}

		protected override ComponentSnapshot OnEventApplied(ComponentEvent @event)
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
			if( Entity.SceneManager.Simulator.IsServer || Character.IsLocalCharacter )
			{
				var s = (CharacterMotionControllerSnapshot)State;
				var velocity = s.InputDirection * s.InputMagnitude * s.MaxSpeed;
				var offset = velocity * Configuration.SimulationDeltaTime / 1000f;

				if( !MathUtils.IsApproximate( velocity, Character.Transform.Velocity ) )
					Character.Transform.ApplyTransformVelocityChangedEvent( velocity );

				if( offset.LengthSquared() > 0 )
					Character.Transform.ApplyTransformMovedEvent( offset );
			}
		}

		protected override void OnSnapshotRecovered(ComponentSnapshot state)
		{
		}

		protected internal override ComponentSnapshot CreateSnapshot()
		{
			var s = CreateSnapshot<CharacterMotionControllerSnapshot>();
			s.InputDirection = new Vector2D( 0, 1 );
			s.InputMagnitude = 0;
			s.MaxSpeed = 4f;
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

		internal CharacterMotionControllerSnapshot TheState => (CharacterMotionControllerSnapshot)State;
	}
}
