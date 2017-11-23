using EcsSync2.Fps;
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

		Vector2D m_joystickDirection = new Vector2D( 0, 1 );
		float m_joystickMagnitude = 0;
		bool[] m_buttons = new bool[3];

		public InputManager(Simulator simulator)
			: base( simulator )
		{
		}

		internal void SetInput()
		{
			var c = (IContext)Simulator.Context;
			var axis = new Vector2D( c.GetAxis( "Horizontal" ), c.GetAxis( "Vertical" ) );
			Debug.Assert( axis.IsValid() );
			m_joystickMagnitude = axis.Length();
			if( m_joystickMagnitude > 0 )
			{
				axis.Normalize();
				m_joystickDirection = axis;
			}
			Debug.Assert( m_joystickDirection.IsValid() );
			Debug.Assert( !float.IsNaN( m_joystickMagnitude ) );

			m_buttons[0] = c.GetButton( "Fire1" );
			m_buttons[1] = c.GetButton( "Fire2" );
			m_buttons[2] = c.GetButton( "Jump" );
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
			frame.Time = Simulator.FixedTime;

			var scene = Simulator.SceneManager.Scene as BattleScene;
			if( scene != null )
			{
				if( scene.LocalCharacter != null )
				{
					MoveCharacterCommand( frame, scene.LocalCharacter.MotionController );
				}
				else if( scene.LocalPlayer != null )
				{
					if( m_buttons[2] )
						PlayerConnectCommand( frame, scene.LocalPlayer );
				}
			}

			Simulator.CommandQueue.Add( frame.UserId, frame );

			frame.Release();
			return frame;
		}

		void PlayerConnectCommand(CommandFrame frame, Player player)
		{
			var c = frame.AddCommand<PlayerConnectCommand>();
			c.ComponentId = player.ConnectionManager.Id;
		}

		void MoveCharacterCommand(CommandFrame frame, CharacterMotionController motion)
		{
			var c = frame.AddCommand<MoveCharacterCommand>();
			c.ComponentId = motion.Id;
			c.InputDirection = new Vector2D( m_joystickDirection[0], m_joystickDirection[1] );
			c.InputMagnitude = m_joystickMagnitude;
		}
	}
}
