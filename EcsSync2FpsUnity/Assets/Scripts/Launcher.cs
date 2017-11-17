using EcsSync2;
using EcsSync2.Fps;
using System;
using UnityEngine;

public class Launcher : MonoBehaviour, Simulator.IContext, InputManager.IContext
{
	public string ServerAddress = "192.168.92.144";
	public int ServerPort = 5000;
	public ulong UserId = 1000;

	public CharacterPawn CharacterPawn;
	FpsClient m_client;

	void Awake()
	{
		m_client = new FpsClient( this, ServerAddress, ServerPort, UserId );
		m_client.OnLogin += OnLogin;
	}

	void OnLogin(Simulator simulator)
	{
		Debug.LogFormat( "OnLogin" );

		simulator.SceneManager.OnSceneLoaded += SceneManager_OnSceneLoaded;
	}

	void SceneManager_OnSceneLoaded(Scene scene)
	{
		Debug.LogFormat( "SceneManager_OnSceneLoaded" );

		scene.OnEntityCreated += Scene_OnEntityCreated;
		scene.OnEntityRemoved += Scene_OnEntityRemoved;
	}

	void Scene_OnEntityCreated(Entity entity)
	{
		Debug.LogFormat( "Scene_OnEntityCreated {0}", entity );

		if( entity is Character )
		{
			var c = (Character)entity;
			var go = Instantiate( CharacterPawn.gameObject, c.Transform.Position.ToUnityPos(), Quaternion.identity );
			var pawn = go.GetComponent<CharacterPawn>();
			pawn.Initialize( c );
		}
	}

	void Scene_OnEntityRemoved(Entity entity)
	{
		Debug.LogFormat( "Scene_OnEntityRemoved {0}", entity );
	}

	void OnDestroy()
	{
		m_client.Stop();
	}

	void Update()
	{
		m_client.Update();
	}

	#region Simulator.IContext

	public void Log(string msg, params object[] args)
	{
		msg = string.Format( msg, args );
		msg = string.Format( "{0} {1}", DateTime.Now.ToString( "HH:mm:ss.fff" ), msg );
		Debug.Log( msg, this );
	}

	public void LogError(string msg, params object[] args)
	{
		msg = string.Format( msg, args );
		msg = string.Format( "{0} {1}", DateTime.Now.ToString( "HH:mm:ss.fff" ), msg );
		Debug.LogError( msg, this );
	}

	public void LogWarning(string msg, params object[] args)
	{
		msg = string.Format( msg, args );
		msg = string.Format( "{0} {1}", DateTime.Now.ToString( "HH:mm:ss.fff" ), msg );
		Debug.LogWarning( msg, this );
	}

	#endregion

	#region InputManager.IContext

	float InputManager.IContext.GetAxis(string name)
	{
		return Input.GetAxis( name );
	}

	bool InputManager.IContext.GetButton(string name)
	{
		return Input.GetButton( name );
	}

	#endregion
}
