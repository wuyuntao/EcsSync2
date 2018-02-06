using EcsSync2.Fps;
using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace EcsSync2.FpsUnity
{
	public class SimulatorContext : MonoBehaviour, Simulator.IContext, InputManager.IContext, NetworkClient.IContext, RenderManager.IContext
	{
		public CharacterCamera CharacterCameraPrefab;
		public CharacterPawn CharacterPawnPrefab;
		public GameObject UICanvasPrefab;
		public GameObject Level;

		public CharacterCamera Camera { get; private set; }
		public UIStatus UIStatus { get; private set; }

		void Start()
		{
			Instantiate( Level, transform );
			Camera = Instantiate( CharacterCameraPrefab, transform ).GetComponent<CharacterCamera>();

			var uiCanvas = Instantiate( UICanvasPrefab );
			UIStatus = uiCanvas.transform.Find( "Status" ).GetComponent<UIStatus>();
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

		bool InputManager.IContext.GetButtonUp(string name)
		{
			return CrossPlatformInputManager.GetButtonUp( name );
		}

		bool InputManager.IContext.GetButtonDown(string name)
		{
			return CrossPlatformInputManager.GetButtonDown( name );
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
					Camera.FollowTarget = pawn.CameraPod;
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

		NetworkClient.INetworkClient NetworkClient.IContext.CreateClient()
		{
			return new LiteNetClient( this );
		}

		#endregion
	}
}
