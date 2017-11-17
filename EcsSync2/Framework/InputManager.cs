using EcsSync2.Fps;
using System;

namespace EcsSync2
{
	public class InputManager : SimulatorComponent
	{
		public interface IContext
		{
			float GetAxis(string name);

			bool GetButton(string name);
		}

		float[] m_joystickDirection = new float[2];
		float m_joystickMagnitude = 0;
		bool[] m_buttons = new bool[3];

		public InputManager(Simulator simulator)
			: base( simulator )
		{
		}

		internal void SetInput()
		{
			var c = (IContext)Simulator.Context;
			m_joystickDirection[0] = c.GetAxis( "Horizontal" );
			m_joystickDirection[1] = c.GetAxis( "Vertical" );
			m_joystickMagnitude = (float)Math.Sqrt( m_joystickDirection[0] * m_joystickDirection[0] + m_joystickDirection[1] + m_joystickDirection[1] );

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

		internal CommandFrame EnqueueCommands()
		{
			var frame = Simulator.ReferencableAllocator.Allocate<CommandFrame>();
			frame.Time = Simulator.FixedTime;
			frame.Retain();

			var scene = Simulator.SceneManager.Scene as BattleScene;
			if( scene != null )
			{
				if( scene.LocalCharacter != null )
				{
					MoveCharacter( frame, scene.LocalCharacter.MotionController );
				}
				else if( scene.LocalPlayer != null )
				{
					if( m_buttons[2] )
						CreateCharacter( frame, scene.LocalPlayer );
				}
			}

			Simulator.CommandQueue.EnqueueCommands( Simulator.LocalUserId.Value, frame );

			frame.Release();
			return frame;
		}

		void CreateCharacter(CommandFrame frame, Player player)
		{
			var c = frame.AddCommand<CreateEntityCommand>();
			c.Settings = new CharacterSettings()
			{
				UserId = player.TheSettings.UserId,
			};
		}

		void MoveCharacter(CommandFrame frame, CharacterMotionController motion)
		{
			var c = frame.AddCommand<MoveCharacterCommand>();
			c.ComponentId = motion.Id;
			c.InputDirection = new Vector2D( m_joystickDirection[0], m_joystickDirection[1] );
			c.InputMagnitude = m_joystickMagnitude;
		}
	}
}
