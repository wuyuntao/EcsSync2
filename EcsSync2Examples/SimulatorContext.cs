using EcsSync2.Fps;
using System;
using System.Collections.Generic;

namespace EcsSync2.Examples
{
	interface INetwork : NetworkServer.IContext, NetworkClient.IContext
	{
	}

	class SimulatorContext : Simulator.IContext, InputManager.IContext, RenderManager.IContext, NetworkServer.IContext, NetworkClient.IContext
	{
		float[] m_axis = new float[2];
		bool[] m_buttons = new bool[3];
		Dictionary<InstanceId, FakeEntityPawn> m_pawns = new Dictionary<InstanceId, FakeEntityPawn>();
		INetwork m_network;

		public SimulatorContext(INetwork network)
		{
			m_network = network;
		}

		NetworkServer.INetworkServer NetworkServer.IContext.CreateServer()
		{
			return m_network.CreateServer();
		}

		NetworkClient.INetworkClient NetworkClient.IContext.CreateClient()
		{
			return m_network.CreateClient();
		}

		float InputManager.IContext.GetAxis(string name)
		{
			switch( name )
			{
				case "Horizontal":
					return m_axis[0];

				case "Vertical":
					return m_axis[1];

				default:
					throw new NotSupportedException( name );
			}
		}

		public float SetAxis(string name, float value)
		{
			switch( name )
			{
				case "Horizontal":
					return m_axis[0] = Math.Clamp( value, 0f, 1f );

				case "Vertical":
					return m_axis[1] = Math.Clamp( value, 0f, 1f );

				default:
					throw new NotSupportedException( name );
			}
		}

		bool InputManager.IContext.GetButton(string name)
		{
			switch( name )
			{
				case "Fire1":
					return m_buttons[0];

				case "Fire2":
					return m_buttons[1];

				case "Jump":
					return m_buttons[2];

				default:
					throw new NotSupportedException( name );
			}
		}

		bool InputManager.IContext.GetButtonUp(string name)
		{
			switch( name )
			{
				case "Fire1":
					return m_buttons[0];

				case "Fire2":
					return m_buttons[1];

				case "Jump":
					return m_buttons[2];

				default:
					throw new NotSupportedException( name );
			}
		}

		bool InputManager.IContext.GetButtonDown(string name)
		{
			switch( name )
			{
				case "Fire1":
					return m_buttons[0];

				case "Fire2":
					return m_buttons[1];

				case "Jump":
					return m_buttons[2];

				default:
					throw new NotSupportedException( name );
			}
		}

		public void SetButton(string name, bool value)
		{
			switch( name )
			{
				case "Fire1":
					m_buttons[0] = value;
					break;

				case "Fire2":
					m_buttons[1] = value;
					break;

				case "Jump":
					m_buttons[2] = value;
					break;

				default:
					throw new NotSupportedException( name );
			}
		}

		public void ResetButtons()
		{
			Array.Clear( m_buttons, 0, m_buttons.Length );
		}

		void ILogger.Log(string msg, params object[] args)
		{
			Logger.Log( msg, args );
		}

		void ILogger.LogError(string msg, params object[] args)
		{
			Logger.LogError( msg, args );
		}

		void ILogger.LogWarning(string msg, params object[] args)
		{
			Logger.LogWarning( msg, args );
		}

		void RenderManager.IContext.CreateEntityPawn(Entity entity)
		{
			if( entity is Character )
			{
				var pawn = new FakeEntityPawn( entity );
				m_pawns.Add( entity.Id, pawn );
			}
		}

		void RenderManager.IContext.DestroyEntityPawn(Entity entity)
		{
			if( entity.Context is FakeEntityPawn )
				m_pawns.Remove( entity.Id );
		}
	}
}
