﻿using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public class SceneManager : SimulatorComponent
	{
		Scene m_scene;
		Dictionary<InstanceId, Entity> m_entities = new Dictionary<InstanceId, Entity>();
		Dictionary<InstanceId, Component> m_components = new Dictionary<InstanceId, Component>();

		public SceneManager(Simulator simulator)
			: base( simulator )
		{
		}

		internal Component FindComponent(InstanceId id)
		{
			m_components.TryGetValue( id, out Component component );
			return component;
		}

		internal Entity CreateEntity(InstanceId id, EntitySettings settings)
		{
			throw new NotImplementedException();
		}

		internal List<Component> GetPredictedComponents()
		{
			throw new NotImplementedException();
		}

		public Scene Scene => m_scene;
	}
}
