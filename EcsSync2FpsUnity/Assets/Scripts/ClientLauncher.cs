using System;
using EcsSync2.Fps;
using UnityEngine;

namespace EcsSync2.FpsUnity
{
	public class ClientLauncher : MonoBehaviour
	{
		public string ServerAddress = "192.168.92.144";
		public int ServerPort = 3687;
		public ulong UserId = 1000;

		public ScenePawn ScenePawn;
		public Camera Camera;
		public UIStatus UIStatus;

		SimulatorContext m_simulatorContext;
		Simulator m_simulator;

		void Awake()
		{
			m_simulatorContext = new SimulatorContext();
			m_simulator = new Simulator( m_simulatorContext, false, true, null, UserId );

			var go = Instantiate( ScenePawn.gameObject );
			var pawn = go.GetComponent<ScenePawn>();
			pawn.Camera = Camera;
			pawn.Initialize( m_simulator );

			m_simulator.SceneManager.LoadScene<BattleScene>();
			m_simulator.NetworkClient.Start( ServerAddress, ServerPort );

			m_simulator.NetworkClient.OnLogin += OnLogin;
		}

		void OnLogin()
		{
			Debug.Log( "OnLogin" );
		}

		void Start()
		{
			InvokeRepeating( nameof( UpdateStatus ), 0.1f, 0.1f );
		}

		void UpdateStatus()
		{
			if( m_simulator != null )
			{
				UIStatus.RTT = m_simulator.SynchronizedClock.Rtt;
			}
		}

		void Update()
		{
			m_simulator.Simulate( Time.deltaTime );
		}
	}
}
