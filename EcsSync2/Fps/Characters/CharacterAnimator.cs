using System;

namespace EcsSync2.Fps
{
	public sealed class CharacterAnimator : Animator
	{
		protected override void OnStart()
		{
			base.OnStart();

			if( Entity.IsLocalEntity )
			{
				var c = (Character)Entity;
				c.Jumper.OnJumpStarted.AddHandler( OnJumpStarted );
			}
		}

		void OnJumpStarted(Jumper jumper)
		{
			ApplyAnimatorStateChangedEvent( "Jump" );
		}

		protected override void OnFixedUpdate()
		{
			base.OnFixedUpdate();

			if( Entity.IsLocalEntity )
			{
				var c = (Character)Entity;
				if( !c.Jumper.IsJumping )
				{
					var isWalking = c.MotionController.TheState.InputMagnitude > 0;
					if( isWalking && TheState.StateName != "Walk" )
					{
						ApplyAnimatorStateChangedEvent( "Walk" );
					}
					else if( !isWalking && TheState.StateName != "Idle" )
					{
						ApplyAnimatorStateChangedEvent( "Idle" );
					}
				}
			}
		}

		protected internal override ComponentSnapshot CreateSnapshot()
		{
			var s = (AnimatorSnapshot)base.CreateSnapshot();
			s.StateName = "Idle";
			return s;
		}
	}
}
