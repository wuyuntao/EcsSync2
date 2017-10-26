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

		public SceneManager SceneManager { get; private set; }
		public InstanceId Id { get; private set; }
		public List<Component> Components { get; } = new List<Component>();

		State m_state = State.Initial;

		internal void Initialize(SceneManager sceneManager, InstanceId id)
		{
			SceneManager = sceneManager;
			Id = id;

			OnInitialize();
		}

		protected abstract void OnInitialize();

		protected T AddComponent<T>()
			where T : Component, new()
		{
			if( SceneManager == null )
				throw new InvalidOperationException( "Not initialized yet" );

			if( m_state != State.Initial )
				throw new InvalidOperationException( "Aready started" );

			var component = new T();
			component.OnInitialize( this, Id );     // TODO How to allocate id
			Components.Add( component );
			return component;
		}
	}
}
