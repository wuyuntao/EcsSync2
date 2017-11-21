using EcsSync2.Fps;
using MessagePack;
using System;
using System.Collections.Generic;

namespace EcsSync2
{
	[Union( 0, typeof( CharacterSettings ) )]
	[Union( 1, typeof( GameManagerSettings ) )]
	[Union( 2, typeof( ItemSettings ) )]
	[Union( 3, typeof( PlayerSettings ) )]
	[Union( 4, typeof( SceneElementSettings ) )]
	public interface IEntitySettings
	{
	}

	public abstract class Entity : Disposable
	{
		public enum State
		{
			Initial,
			Started,
			Destroyed
		}

		public SceneManager SceneManager { get; private set; }
		public InstanceId Id { get; private set; }
		public IEntitySettings Settings { get; private set; }
		public List<Component> Components { get; } = new List<Component>();

		State m_state = State.Initial;

		internal void Initialize(SceneManager sceneManager, InstanceId id, IEntitySettings settings)
		{
			SceneManager = sceneManager;
			Id = id;
			Settings = settings;

			OnInitialize();
		}

		protected abstract void OnInitialize();

		internal void Start()
		{
			foreach( var component in Components )
				component.Start();
		}

		internal void Destroy()
		{
			foreach( var component in Components )
				component.Destroy();
		}

		protected override void DisposeManaged()
		{
			SafeDispose( Components );

			base.DisposeManaged();
		}

		protected T AddComponent<T>(ComponentSettings settings = null)
			where T : Component, new()
		{
			if( SceneManager == null )
				throw new InvalidOperationException( "Not initialized yet" );

			if( m_state != State.Initial )
				throw new InvalidOperationException( "Aready started" );

			var component = new T();
			component.Initialize( this, Id.CreateComponentId( (uint)( Components.Count + 1 ) ), settings );
			Components.Add( component );
			return component;
		}

		internal EntitySnapshot CreateSnapshot()
		{
			var s = SceneManager.Simulator.ReferencableAllocator.Allocate<EntitySnapshot>();
			s.Id = Id;
			s.Settings = Settings;

			foreach( var c in Components )
			{
				var cs = c.CreateSnapshot();
				s.Components.Add( cs );
				cs.Retain();
			}

			return s;
		}
	}
}
