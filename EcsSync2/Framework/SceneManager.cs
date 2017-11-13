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

		public T LoadScene<T>()
			where T : Scene, new()
		{
			var scene = new T();
			m_scene = scene;
			m_scene.Initialize( this );
			return scene;
		}

		public T CreateEntity<T>(InstanceId id, EntitySettings settings)
			where T : Entity, new()
		{
			var entity = new T();
			entity.Initialize( this, id, settings );
			m_entities.Add( id, entity );
			foreach( var component in entity.Components )
				m_components.Add( component.Id, component );
			entity.Start();
			return entity;
		}

		internal bool RemoveEntity(InstanceId id)
		{
			var entity = FindEntity( id );
			if( entity == null )
				return false;

			entity.Destroy();
			m_entities.Remove( id );
			foreach( var component in entity.Components )
				m_components.Remove( component.Id );
			return true;
		}

		internal Entity FindEntity(InstanceId id)
		{
			m_entities.TryGetValue( id, out Entity entity );
			return entity;
		}

		internal Component FindComponent(InstanceId id)
		{
			m_components.TryGetValue( id, out Component component );
			return component;
		}

		public Scene Scene => m_scene;

		public IEnumerable<Entity> Entities => m_entities.Values;
	}
}
