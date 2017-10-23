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

		internal override void OnUpdate()
		{
			base.OnUpdate();

			var c = Simulator.Context;
			m_joystickDirection[0] = c.GetAxis( "Horizontal" );
			m_joystickDirection[1] = c.GetAxis( "Vertical" );
			m_joystickMagnitude = (float)Math.Sqrt( m_joystickDirection[0] * m_joystickDirection[0] + m_joystickDirection[1] + m_joystickDirection[1] );

			m_buttons[0] = c.GetButton( "Skill1" );
			m_buttons[1] = c.GetButton( "Skill2" );
			m_buttons[2] = c.GetButton( "Abort" );
		}

		internal override void OnFixedUpdate()
		{
			base.OnFixedUpdate();

			var frame = Simulator.ReferencableAllocator.Allocate<CommandFrame>();

			Simulator.CommandDispatcher.Enqueue( Simulator.LocalUserId.Value, frame.Value );

			frame.Release();
		}

		internal override void OnLateUpdate()
		{
			base.OnLateUpdate();

			m_joystickMagnitude = 0;
			m_buttons[0] = false;
			m_buttons[1] = false;
			m_buttons[2] = false;
		}
	}
}
