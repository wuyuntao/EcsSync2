using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public class InputManager : SimulatorComponent
	{
		public delegate void InputHandler(CommandFrame frame);

		public interface IContext
		{
			float GetAxis(string name);

			bool GetButton(string name);
			bool GetButtonUp(string m_button);
			bool GetButtonDown(string m_button);
		}

		IContext m_context;
		SortedList<string, Input> m_inputs = new SortedList<string, Input>();
		List<InputHandler> m_handlers = new List<InputHandler>();

		public InputManager(Simulator simulator)
			: base( simulator )
		{
			m_context = (IContext)Simulator.Context;
		}

		public void RegisterJoystick(string name, string axis0, string axis1)
		{
			if( m_inputs.ContainsKey( name ) )
				throw new InvalidOperationException( $"Input '{name}' already exists" );

			m_inputs.Add( name, new Joystick( this, axis0, axis1 ) );
		}

		public void RegisterButton(string name, string button)
		{
			if( m_inputs.ContainsKey( name ) )
				throw new InvalidOperationException( $"Input '{name}' already exists" );

			m_inputs.Add( name, new Button( this, button ) );
		}

		public void UnregisterInput(string name)
		{
			m_inputs.Remove( name );
		}

		TInput GetInput<TInput>(string name)
			where TInput : Input
		{
			m_inputs.TryGetValue( name, out var input );
			return input as TInput;
		}

		public Joystick GetJoystick(string name)
		{
			return GetInput<Joystick>( name );
		}

		public Button GetButton(string name)
		{
			return GetInput<Button>( name );
		}

		public void RegisterHandler(InputHandler handler)
		{
			m_handlers.Add( handler );
		}

		public void UnregisterHandler(InputHandler handler)
		{
			m_handlers.Remove( handler );
		}

		internal CommandFrame CreateCommands()
		{
			foreach( var input in m_inputs.Values )
				input.Read();

			var frame = Simulator.ReferencableAllocator.Allocate<CommandFrame>();
			frame.UserId = Simulator.LocalUserId.Value;
			frame.Time = Simulator.TickScheduler.CurrentContext.Value.LocalTime;
			//Simulator.Context.Log( "CreateCommands {0} / {1}", Simulator.FixedTime, frame.Time );

			foreach( var handler in m_handlers )
				handler( frame );

			Simulator.CommandQueue.Add( frame.UserId, frame );

			frame.Release();
			return frame;
		}

		public abstract class Input
		{
			internal Input(InputManager inputManager)
			{
				InputManager = inputManager;
			}

			internal abstract void Read();

			internal InputManager InputManager { get; }
		}

		public class Joystick : Input
		{
			string m_xAxis;
			string m_yAxis;

			internal Joystick(InputManager inputManager, string xAxis, string yAxis)
				: base( inputManager )
			{
				m_xAxis = xAxis;
				m_yAxis = yAxis;
			}

			internal override void Read()
			{
				var x = InputManager.m_context.GetAxis( m_xAxis );
				var y = InputManager.m_context.GetAxis( m_yAxis );
				var axis = new Vector2D( x, y );

				Magnitude = axis.Length();
				if( Magnitude > 0 )
				{
					axis.Normalize();
					Direction = axis;
				}
			}

			public Vector2D Direction { get; private set; } = new Vector2D( 0, 1 );

			public float Magnitude { get; private set; }
		}

		public class Button : Input
		{
			string m_button;

			internal Button(InputManager inputManager, string button)
				: base( inputManager )
			{
				m_button = button;
			}

			internal override void Read()
			{
				Up = InputManager.m_context.GetButtonUp( m_button );
				Down = InputManager.m_context.GetButtonDown( m_button );
				Press = InputManager.m_context.GetButton( m_button );
			}

			public bool Up { get; private set; }

			public bool Down { get; private set; }

			public bool Press { get; private set; }
		}
	}
}
