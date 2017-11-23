using System;
using EcsSync2.Fps;
using UnityEngine;

namespace EcsSync2.FpsUnity
{
	public class ClientLauncher : MonoBehaviour
	{
		public string ServerAddress = "192.168.92.144";
		public int ServerPort = 5000;
		public ulong UserId = 1000;

		public ScenePawn ScenePawn;
		public Camera Camera;
		public UIStatus UIStatus;

		FpsClient m_client;

		void Awake()
		{
			m_client = new FpsClient( new SimulatorContext(),
				ServerAddress, ServerPort, UserId );
			m_client.OnLogin += OnLogin;
		}

		void Start()
		{
			InvokeRepeating( nameof( UpdateStatus ), 0.1f, 0.1f );
		}

		void UpdateStatus()
		{
			if( m_client.Simulator != null )
			{
				UIStatus.RTT = m_client.Simulator.SynchronizedClock.Rtt;
			}
		}

		void OnLogin(Simulator simulator)
		{
			Debug.LogFormat( "OnLogin" );

			var go = Instantiate( ScenePawn.gameObject );
			var pawn = go.GetComponent<ScenePawn>();
			pawn.Camera = Camera;
			pawn.Initialize( simulator );
		}

		void OnDestroy()
		{
			m_client.Stop();
		}

		void Update()
		{
			m_client.Update();
		}
	}
}
