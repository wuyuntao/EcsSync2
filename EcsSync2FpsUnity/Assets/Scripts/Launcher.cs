using EcsSync2;
using EcsSync2.Fps;
using UnityEngine;

public class Launcher : MonoBehaviour, Simulator.IContext, InputManager.IContext
{
	public string ServerAddress = "192.168.92.144";
	public int ServerPort = 5000;
	public ulong UserId = 1000;

	FpsClient m_client;

	void Awake()
	{
		m_client = new FpsClient( this, ServerAddress, ServerPort, UserId );
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

	void EcsSync2.ILogger.Log(string msg, params object[] args)
	{
		Debug.LogFormat( msg, args );
	}

	void EcsSync2.ILogger.LogError(string msg, params object[] args)
	{
		Debug.LogErrorFormat( msg, args );
	}

	void EcsSync2.ILogger.LogWarning(string msg, params object[] args)
	{
		Debug.LogWarningFormat( msg, args );
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
