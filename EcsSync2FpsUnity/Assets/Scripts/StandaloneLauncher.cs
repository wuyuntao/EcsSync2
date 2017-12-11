using EcsSync2.Fps;
using UnityEngine;

namespace EcsSync2.FpsUnity
{
	public class StandaloneLauncher : MonoBehaviour
	{
		public int Seed = 12345;
		public ulong UserId = 1000;

		public SimulatorContext SceneRoot;

		Simulator m_simulator;

		void Awake()
		{
			var go = Instantiate( SceneRoot.gameObject );
			var context = go.GetComponent<SimulatorContext>();
			context.IsStandalone = true;

			m_simulator = new Simulator( context,
				true, true, Seed, UserId );
			m_simulator.SceneManager.LoadScene<BattleScene>();
		}

		void Start()
		{
			var f = m_simulator.ReferencableAllocator.Allocate<CommandFrame>();
			f.Time = m_simulator.StandaloneTickScheduler.Time + Configuration.SimulationDeltaTime;
			f.Retain();
			var c = f.AddCommand<CreateEntityCommand>();
			c.Settings = new PlayerSettings() { UserId = UserId };

			m_simulator.CommandQueue.Add( 0, f );

			Debug.LogFormat( "CreatePlayer {0}", Time.time );
		}

		void Update()
		{
			m_simulator.Simulate( Time.deltaTime );
		}
	}
}
