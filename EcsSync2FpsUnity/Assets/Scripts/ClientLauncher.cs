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

		public SimulatorContext SceneRoot = null;

		SimulatorContext m_simulatorContext;
		Simulator m_simulator;

		void Awake()
		{
			var go = Instantiate( SceneRoot.gameObject );
			m_simulatorContext = go.GetComponent<SimulatorContext>();
			m_simulatorContext.Client = new LiteNetClient( m_simulatorContext );

			m_simulator = new Simulator( m_simulatorContext, false, true, null, UserId );
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
			if( m_simulatorContext.UIStatus != null )
			{
				m_simulatorContext.UIStatus.RTT = m_simulator.SynchronizedClock.Rtt;
				m_simulatorContext.UIStatus.IND = m_simulator.RenderManager.InterpolationDelay / 1000f;
			}
		}

		void Update()
		{
			m_simulator.Simulate( Time.deltaTime );
		}
	}
}
