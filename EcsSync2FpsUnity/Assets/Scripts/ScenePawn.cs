using EcsSync2.Fps;
using UnityEngine;

namespace EcsSync2.FpsUnity
{
    public class ScenePawn : MonoBehaviour
    {
        public CharacterPawn CharacterPawn;

        Simulator m_simulator;

        public void Initialize(Simulator simulator)
        {
            m_simulator = simulator;
            m_simulator.SceneManager.OnSceneLoaded += SceneManager_OnSceneLoaded;
            m_simulator.SceneManager.LoadScene<BattleScene>();
        }

        void SceneManager_OnSceneLoaded(Scene scene)
        {
            Debug.LogFormat("SceneManager_OnSceneLoaded");

            scene.OnEntityCreated += Scene_OnEntityCreated;
            scene.OnEntityRemoved += Scene_OnEntityRemoved;
        }

        void Scene_OnEntityCreated(Entity entity)
        {
            Debug.LogFormat("Scene_OnEntityCreated {0}", entity);

            if (entity is Character)
            {
                var c = (Character)entity;
                var go = Instantiate(CharacterPawn.gameObject, c.Transform.Position.ToUnityPos(), Quaternion.identity);
                var pawn = go.GetComponent<CharacterPawn>();
                pawn.Initialize(c);
            }
        }

        void Scene_OnEntityRemoved(Entity entity)
        {
            Debug.LogFormat("Scene_OnEntityRemoved {0}", entity);
        }
    }
}
