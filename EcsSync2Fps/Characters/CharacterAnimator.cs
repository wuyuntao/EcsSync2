using System;

namespace EcsSync2.Fps
{
	public sealed class CharacterAnimator : Animator
	{
		protected override void OnStart()
		{
			base.OnStart();

			if( Entity.SceneManager.Simulator.IsServer || Entity.IsLocalEntity )
			{
				var c = (Character)Entity;
				c.Jumper.OnJumpStarted.AddHandler( OnJumpStarted );
			}
		}

		void OnJumpStarted(Jumper jumper, uint jumpContext)
		{
			ApplyAnimatorStateChangedEvent( "Jump", jumpContext );
		}

		protected override void OnFixedUpdate()
		{
			base.OnFixedUpdate();

			if( Entity.SceneManager.Simulator.IsServer || Entity.IsLocalEntity )
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

		protected override ComponentSnapshot CreateSnapshot()
		{
			var s = (AnimatorSnapshot)base.CreateSnapshot();
			s.StateName = "Idle";
			return s;
		}
	}
}
