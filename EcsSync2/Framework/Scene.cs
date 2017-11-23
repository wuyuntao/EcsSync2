using MessagePack;
using System;

namespace EcsSync2
{
	[MessagePackObject]
	public class CreateEntityCommand : SceneCommand
	{
		[Key( 10 )]
		public IEntitySettings Settings;

		protected override void OnReset()
		{
			Settings = null;
		}
	}

	[MessagePackObject]
	public class RemoveEntityCommand : SceneCommand
	{
		[Key( 10 )]
		public uint EntityId;

		protected override void OnReset()
		{
			EntityId = 0;
		}
	}

	[MessagePackObject]
	public class EntityCreatedEvent : SceneEvent
	{
		[Key( 10 )]
		public uint EntityId;

		[Key( 11 )]
		public IEntitySettings Settings;

		protected override void OnReset()
		{
			EntityId = 0;
			Settings = null;
		}
	}

	[MessagePackObject]
	public class EntityRemovedEvent : SceneEvent
	{
		[Key( 10 )]
		public uint EntityId;

		protected override void OnReset()
		{
			EntityId = 0;
		}
	}

	public abstract class Scene
	{
		public Action<Entity> OnEntityCreated;
		public Action<Entity> OnEntityRemoved;

		public SceneManager SceneManager { get; private set; }

		internal virtual void Initialize(SceneManager sceneManager)
		{
			SceneManager = sceneManager;

			OnInitialize();
		}

		protected abstract void OnInitialize();

		protected internal abstract void CreateEntity(InstanceId id, IEntitySettings settings);

		protected TEntity CreateEntity<TEntity, TEntitySettings>(InstanceId id, IEntitySettings settings)
			where TEntity : Entity, new()
			where TEntitySettings : IEntitySettings
		{
			return SceneManager.CreateEntity<TEntity>( id, settings );
		}

		public void ApplyEntityCreatedEvent(IEntitySettings settings)
		{
			var e = SceneManager.Simulator.ReferencableAllocator.Allocate<EntityCreatedEvent>();
			e.EntityId = SceneManager.Simulator.InstanceIdAllocator.Allocate();
			e.Settings = settings;
			ApplyEvent( e );

			SceneManager.Simulator.Context.Log( "ApplyEntityCreatedEvent {0} {1}", e.EntityId, e.Settings );
		}

		public void ApplyEntityRemovedEvent(InstanceId id)
		{
			var e = SceneManager.Simulator.ReferencableAllocator.Allocate<EntityRemovedEvent>();
			e.EntityId = SceneManager.Simulator.InstanceIdAllocator.Allocate();
			ApplyEvent( e );

			SceneManager.Simulator.Context.Log( "ApplyEntityRemovedEvent {0}", e.EntityId );
		}

		internal void ReceiveCommand(SceneCommand command)
		{
			OnCommandReceived( command );
		}

		protected virtual void OnCommandReceived(SceneCommand command)
		{
			if( !SceneManager.Simulator.IsServer )
				return;

			switch( command )
			{
				case CreateEntityCommand c:
					ApplyEntityCreatedEvent( c.Settings );
					break;

				case RemoveEntityCommand c:
					ApplyEntityRemovedEvent( c.EntityId );
					break;

				default:
					throw new NotSupportedException( command.ToString() );
			}
		}

		internal void ApplyEvent(SceneEvent @event)
		{
			OnEventApplied( @event );

			if( SceneManager.Simulator.ServerTickScheduler != null )
				SceneManager.Simulator.EventBus.EnqueueEvent( SceneManager.Simulator.TickScheduler.CurrentContext.Value.Time, @event );

			@event.Release();
		}

		protected virtual void OnEventApplied(SceneEvent @event)
		{
			switch( @event )
			{
				case EntityCreatedEvent e:
					CreateEntity( e.EntityId, e.Settings );
					break;

				case EntityRemovedEvent e:
					SceneManager.RemoveEntity( e.EntityId );
					break;

				default:
					throw new NotSupportedException( @event.ToString() );
			}
		}
	}
}
