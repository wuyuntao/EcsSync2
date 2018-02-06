using EcsSync2.Fps;
using System;
using System.Collections.Generic;

namespace EcsSync2.Examples
{
	class SimulatorContext : Simulator.IContext, InputManager.IContext, RenderManager.IContext, NetworkServer.IServerContext, NetworkClient.IClientContext
	{
		NetworkServer.IServerContext m_server;
		NetworkClient.IClientContext m_client;
		float[] m_axis = new float[2];
		bool[] m_buttons = new bool[3];
		Dictionary<InstanceId, FakeEntityPawn> m_pawns = new Dictionary<InstanceId, FakeEntityPawn>();

		public SimulatorContext(NetworkServer.IServerContext server = null, NetworkClient.IClientContext client = null)
		{
			m_server = server;
			m_client = client;
		}

		Action<NetworkManager.IStream> NetworkManager.IContext.OnConnected
		{
			get
			{
				if( m_server != null )
					return m_server.OnConnected;
				else if( m_client != null )
					return m_client.OnConnected;
				else
					throw new NotSupportedException();
			}
			set
			{
				if( m_server != null )
					m_server.OnConnected = value;
				else if( m_client != null )
					m_client.OnConnected = value;
				else
					throw new NotSupportedException();
			}
		}

		Action<NetworkManager.IStream> NetworkManager.IContext.OnDisconnected
		{
			get
			{
				if( m_server != null )
					return m_server.OnDisconnected;
				else if( m_client != null )
					return m_client.OnDisconnected;
				else
					throw new NotSupportedException();
			}
			set
			{
				if( m_server != null )
					m_server.OnDisconnected = value;
				else if( m_client != null )
					m_client.OnDisconnected = value;
				else
					throw new NotSupportedException();
			}
		}

		void NetworkServer.IServerContext.Bind(int port)
		{
			m_server?.Bind( port );
		}

		void NetworkClient.IClientContext.Connect(string address, int port)
		{
			m_client?.Connect( address, port );
		}

		void NetworkManager.IContext.Poll()
		{
			if( m_server != null )
				m_server.Poll();
			else if( m_client != null )
				m_client.Poll();
			else
				throw new NotSupportedException();
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
