using EcsSync2.Fps;
using UnityEngine;

namespace EcsSync2.FpsUnity
{
    public class StandaloneLauncher : MonoBehaviour
    {
        public int Seed = 12345;
        public ulong UserId = 1000;

        public ScenePawn ScenePawn;

        Simulator m_simulator;

        void Awake()
        {
            m_simulator = new Simulator(new SimulatorContext(),
                true, true, Seed, UserId);

            var go = Instantiate(ScenePawn.gameObject);
            var pawn = go.GetComponent<ScenePawn>();
            pawn.Initialize(m_simulator);
        }

        void Start()
        {
            var f = m_simulator.ReferencableAllocator.Allocate<CommandFrame>();
            f.Time = m_simulator.FixedTime + Configuration.SimulationDeltaTime;
            f.Retain();
            var c = f.AddCommand<CreateEntityCommand>();
            c.Settings = new PlayerSettings() { UserId = UserId };

            m_simulator.CommandQueue.EnqueueCommands(0, f);

            Debug.LogFormat("CreatePlayer {0}", Time.time);
        }
        
        void Update()
        {
            m_simulator.Simulate(Time.deltaTime);
        }
    }
}
