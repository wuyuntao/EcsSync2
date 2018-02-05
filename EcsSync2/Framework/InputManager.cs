using System.Diagnostics;

namespace EcsSync2
{
	public class InputManager : SimulatorComponent
	{
		public interface IContext
		{
			float GetAxis(string name);

			bool GetButton(string name);
		}

		IContext m_context;
		Vector2D m_joystickDirection = new Vector2D( 0, 1 );
		float m_joystickMagnitude = 0;
		bool[] m_buttons = new bool[3];

		public InputManager(Simulator simulator)
			: base( simulator )
		{
			m_context = (IContext)Simulator.Context;
		}

		internal void SetInput()
		{
			var axis = new Vector2D( m_context.GetAxis( "Horizontal" ), m_context.GetAxis( "Vertical" ) );
			Debug.Assert( axis.IsValid() );
			m_joystickMagnitude = axis.Length();
			if( m_joystickMagnitude > 0 )
			{
				axis.Normalize();
				m_joystickDirection = axis;
			}
			Debug.Assert( m_joystickDirection.IsValid() );
			Debug.Assert( !float.IsNaN( m_joystickMagnitude ) );

			m_buttons[0] = m_context.GetButton( "Fire1" );
			m_buttons[1] = m_context.GetButton( "Fire2" );
			m_buttons[2] = m_context.GetButton( "Jump" );
		}

		internal void ResetInput()
		{
			m_joystickMagnitude = 0;
			m_buttons[0] = false;
			m_buttons[1] = false;
			m_buttons[2] = false;
		}

		internal CommandFrame CreateCommands()
		{
			var frame = Simulator.ReferencableAllocator.Allocate<CommandFrame>();
			frame.UserId = Simulator.LocalUserId.Value;
			frame.Time = Simulator.TickScheduler.CurrentContext.Value.LocalTime;
			//Simulator.Context.Log( "CreateCommands {0} / {1}", Simulator.FixedTime, frame.Time );

			// TODO Create commands
			//if( Simulator.SceneManager.Scene is BattleScene scene )
			//{
			//	if( scene.LocalCharacter != null )
			//	{
			//		MoveCharacterCommand( frame, scene.LocalCharacter.MotionController );

			//		if( m_buttons[2] )
			//			JumpCommand( frame, scene.LocalCharacter.Jumper );
			//	}
			//	else if( scene.LocalPlayer != null )
			//	{
			//		if( m_buttons[2] )
			//			PlayerConnectCommand( frame, scene.LocalPlayer );
			//	}
			//}

			Simulator.CommandQueue.Add( frame.UserId, frame );

			frame.Release();
			return frame;
		}

		//void PlayerConnectCommand(CommandFrame frame, Player player)
		//{
		//	var c = frame.AddCommand<PlayerConnectCommand>();
		//	c.ComponentId = player.ConnectionManager.Id;

		//	//Simulator.Context.Log( "PlayerConnectCommand {0}, {1}", frame, player );
		//}

		//void MoveCharacterCommand(CommandFrame frame, CharacterMotionController motion)
		//{
		//	var c = frame.AddCommand<MoveCharacterCommand>();
		//	c.ComponentId = motion.Id;
		//	c.InputDirection = new Vector2D( m_joystickDirection[0], m_joystickDirection[1] );
		//	c.InputMagnitude = m_joystickMagnitude;

		//	//Simulator.Context.Log( "MoveCharacterCommand {0}, {1}", frame, motion );
		//}

		//void JumpCommand(CommandFrame frame, Jumper jumper)
		//{
		//	var c = frame.AddCommand<JumpCommand>();
		//	c.ComponentId = jumper.Id;

		//	//Simulator.Context.Log( "JumpCommand {0}, {1}", frame, motion );
		//}
	}
}
