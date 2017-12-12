using EcsSync2.Fps;
using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace EcsSync2.FpsUnity
{
	public class SimulatorContext : MonoBehaviour, Simulator.IContext, InputManager.IContext, NetworkClient.IClientContext, RenderManager.IContext
	{
		public CharacterCamera CharacterCameraPrefab;
		public CharacterPawn CharacterPawnPrefab;
		public GameObject UICanvasPrefab;
		public GameObject Level;
		public bool IsStandalone;

		LiteNetClient m_client;
		CharacterCamera m_camera;

		void Start()
		{
			if( !IsStandalone )
				m_client = new LiteNetClient( this );

			Instantiate( Level, transform );
			Instantiate( UICanvasPrefab );
			m_camera = Instantiate( CharacterCameraPrefab, transform ).GetComponent<CharacterCamera>();
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

		#region RenderManager

		void RenderManager.IContext.CreateEntityPawn(Entity entity)
		{
			if( entity is Character )
			{
				var character = (Character)entity;
				var go = Instantiate( CharacterPawnPrefab.gameObject,
					character.Transform.Position.ToUnityPos(),
					Quaternion.identity, transform );
				var pawn = go.GetComponent<CharacterPawn>();
				pawn.Initialize( character );

				if( character.IsLocalCharacter )
					m_camera.FollowTarget = pawn.CameraPod;
			}
		}

		void RenderManager.IContext.DestroyEntityPawn(Entity entity)
		{
			var context = entity.Context as EntityPawn;
			if( context )
			{
				Destroy( context.gameObject );
			}
		}

		#endregion

		#region NetworkClient.IClientContext

		public Action<NetworkManager.IStream> OnConnected
		{
			get { return m_client.OnConnected; }
			set { m_client.OnConnected = value; }
		}

		public Action<NetworkManager.IStream> OnDisconnected
		{
			get { return m_client.OnDisconnected; }
			set { m_client.OnDisconnected = value; }
		}

		void NetworkClient.IClientContext.Connect(string address, int port)
		{
			m_client.Connect( address, port );
		}

		void NetworkManager.IContext.Poll()
		{
			m_client.Poll();
		}

		#endregion
	}
}
