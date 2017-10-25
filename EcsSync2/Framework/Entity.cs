using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public abstract class EntitySettings
	{
	}

	public abstract class Entity
	{
		public enum State
		{
			Initial,
			Started,
			Destroyed
		}

		public SceneManager Scene { get; private set; }
		public InstanceId Id { get; private set; }

		List<Component> m_components = new List<Component>();
		State m_state = State.Initial;

		internal virtual void OnInitialize(SceneManager scene, InstanceId id)
		{
			Scene = scene;
			Id = id;
		}

		protected T AddComponent<T>()
			where T : Component, new()
		{
			if( Scene == null )
				throw new InvalidOperationException( "Not initialized yet" );

			if( m_state != State.Initial )
				throw new InvalidOperationException( "Aready started" );

			var component = new T();
			component.OnInitialize( this, Id );     // TODO How to allocate id
			m_components.Add( component );
			return component;
		}
	}
}
