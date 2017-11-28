using EcsSync2.Fps;
using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace EcsSync2.FpsUnity
{
	class SimulatorContext : Simulator.IContext, InputManager.IContext, NetworkClient.IClientContext
	{
		LiteNetClient m_client;

		public SimulatorContext()
		{
			m_client = new LiteNetClient( this );
		}

		#region Simulator.IContext

		public void Log(string msg, params object[] args)
		{
			msg = string.Format( msg, args );
			msg = string.Format( "{0} {1}", DateTime.Now.ToString( "HH:mm:ss.fff" ), msg );
			Debug.Log( msg );
		}

		public void LogError(string msg, params object[] args)
		{
			msg = string.Format( msg, args );
			msg = string.Format( "{0} {1}", DateTime.Now.ToString( "HH:mm:ss.fff" ), msg );
			Debug.LogError( msg );
		}

		public void LogWarning(string msg, params object[] args)
		{
			msg = string.Format( msg, args );
			msg = string.Format( "{0} {1}", DateTime.Now.ToString( "HH:mm:ss.fff" ), msg );
			Debug.LogWarning( msg );
		}

		#endregion

		#region InputManager.IContext

		float InputManager.IContext.GetAxis(string name)
		{
			return CrossPlatformInputManager.GetAxis( name );
		}

		bool InputManager.IContext.GetButton(string name)
		{
			return CrossPlatformInputManager.GetButton( name );
		}

		#endregion

		#region NetworkClient.IClientContext

		public Action<NetworkComponet.IStream> OnConnected
		{
			get { return m_client.OnConnected; }
			set { m_client.OnConnected = value; }
		}

		public Action<NetworkComponet.IStream> OnDisconnected
		{
			get { return m_client.OnDisconnected; }
			set { m_client.OnDisconnected = value; }
		}

		public void Connect(string address, int port)
		{
			m_client.Connect( address, port );
		}

		public void Poll()
		{
			m_client.Poll();
		}

		#endregion
	}
}
