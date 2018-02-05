using ProtoBuf;
using System;
using System.Collections.Generic;

namespace EcsSync2
{
	[ProtoContract]
	public abstract class EntitySettings
	{
	}

	public abstract class Entity : Disposable
	{
		public interface IContext
		{
		}

		public enum State
		{
			Initial,
			Started,
			Destroyed
		}

		public SceneManager SceneManager { get; private set; }
		public InstanceId Id { get; private set; }
		public EntitySettings Settings { get; private set; }
		public List<Component> Components { get; } = new List<Component>();
		public IContext Context { get; set; }

		State m_state = State.Initial;

		internal void Initialize(SceneManager sceneManager, InstanceId id, EntitySettings settings)
		{
			SceneManager = sceneManager;
			Id = id;
			Settings = settings;

			OnInitialize();
		}

		public override string ToString()
		{
			return $"{GetType().Name}-{Id}";
		}

		protected abstract void OnInitialize();

		internal void Start()
		{
			if( m_state != State.Initial )
				throw new InvalidOperationException( "Already started" );

			m_state = State.Started;

			foreach( var component in Components )
				component.Start();
		}

		internal void Destroy()
		{
			if( m_state != State.Started )
				throw new InvalidOperationException( "Not started" );

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
				throw new InvalidOperationException( "Already started" );

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
				if( cs != null )
				{
					s.Components.Add( cs );
					cs.Retain();
				}
			}

			return s;
		}

		public virtual bool IsLocalEntity => SceneManager.Simulator.IsServer;
	}
}
