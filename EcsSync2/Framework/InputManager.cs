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
			var c = Simulator.Context;
			m_joystickDirection[0] = c.GetAxis( "Horizontal" );
			m_joystickDirection[1] = c.GetAxis( "Vertical" );
			m_joystickMagnitude = (float)Math.Sqrt( m_joystickDirection[0] * m_joystickDirection[0] + m_joystickDirection[1] + m_joystickDirection[1] );

			m_buttons[0] = c.GetButton( "Skill1" );
			m_buttons[1] = c.GetButton( "Skill2" );
			m_buttons[2] = c.GetButton( "Abort" );
		}

		internal void EnqueueCommands()
		{
			var frame = Simulator.ReferencableAllocator.Allocate<CommandFrame>();
			frame.Time = Simulator.FixedTime;

			var command = frame.AddCommand<MoveCharacterCommand>();
			{
				command.Direction = new float[] { m_joystickDirection[0], m_joystickDirection[1] };
				command.Magnitude = m_joystickMagnitude;
			}

			Simulator.CommandQueue.Enqueue( Simulator.LocalUserId.Value, frame );

			frame.Release();
		}

		internal void ResetInput()
		{
			m_joystickMagnitude = 0;
			m_buttons[0] = false;
			m_buttons[1] = false;
			m_buttons[2] = false;
		}
	}
}
