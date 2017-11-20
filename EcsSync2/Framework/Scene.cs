using EcsSync2.Fps;
using MessagePack;
using System;

namespace EcsSync2
{
	[MessagePackObject]
	public class CreateEntityCommand : SceneCommand
	{
		[Key( 10 )]
		public IEntitySettings Settings;
	}

	[MessagePackObject]
	public class RemoveEntityCommand : SceneCommand
	{
		[Key( 10 )]
		public uint Id;
	}


	[MessagePackObject]
	public class EntityCreatedEvent : SceneEvent, IEvent
	{
		[Key( 10 )]
		public uint Id;

		[Key( 11 )]
		public IEntitySettings Settings;
	}

	[MessagePackObject]
	public class EntityRemovedEvent : SceneEvent, IEvent
	{
		[Key( 10 )]
		public uint Id;
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

		protected internal abstract Entity CreateEntity(InstanceId id, IEntitySettings settings);

		protected TEntity CreateEntity<TEntity, TEntitySettings>(InstanceId id, IEntitySettings settings)
			where TEntity : Entity, new()
			where TEntitySettings : IEntitySettings
		{
			return SceneManager.CreateEntity<TEntity>( id, settings );
		}

		public void ApplyEntityCreatedEvent(IEntitySettings settings)
		{
			var e = SceneManager.Simulator.ReferencableAllocator.Allocate<EntityCreatedEvent>();
			e.Id = SceneManager.Simulator.InstanceIdAllocator.Allocate();
			e.Settings = settings;
			ApplyEvent( e );

			SceneManager.Simulator.Context.Log( "ApplyEntityCreatedEvent {0} {1}", e.Id, e.Settings );
		}

		public void ApplyEntityRemovedEvent(InstanceId id)
		{
			var e = SceneManager.Simulator.ReferencableAllocator.Allocate<EntityRemovedEvent>();
			e.Id = SceneManager.Simulator.InstanceIdAllocator.Allocate();
			ApplyEvent( e );

			SceneManager.Simulator.Context.Log( "ApplyEntityRemovedEvent {0}", e.Id );
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
					ApplyEntityRemovedEvent( c.Id );
					break;

				default:
					throw new NotSupportedException( command.ToString() );
			}
		}

		internal void ApplyEvent(SceneEvent @event)
		{
			OnEventApplied( @event );
			SceneManager.Simulator.EventBus.EnqueueEvent( SceneManager.Simulator.TickScheduler.CurrentContext.Value.Time, @event );
			@event.Release();
		}

		protected virtual void OnEventApplied(SceneEvent @event)
		{
			switch( @event )
			{
				case EntityCreatedEvent e:
					CreateEntity( e.Id, e.Settings );
					break;

				case EntityRemovedEvent e:
					SceneManager.RemoveEntity( e.Id );
					break;

				default:
					throw new NotSupportedException( @event.ToString() );
			}
		}
	}
}
