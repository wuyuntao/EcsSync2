using System.Collections.Generic;

namespace EcsSync2
{
	public class SceneManager : SimulatorComponent
	{
		Scene m_scene;
		Dictionary<InstanceId, Entity> m_entities = new Dictionary<InstanceId, Entity>();
		Dictionary<InstanceId, Component> m_components = new Dictionary<InstanceId, Component>();

		internal Component FindComponent(InstanceId id)
		{
			m_components.TryGetValue( id, out Component component );
			return component;
		}
	}
}
