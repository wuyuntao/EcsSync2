using System;

namespace EcsSync2
{

	public class CreateEntityCommand : SceneCommand
	{
		public EntitySettings Settings;
	}

	public class RemoveEntityCommand : SceneCommand
	{
		public InstanceId Id;
	}


	public class EntityCreatedEvent : SceneEvent
	{
		public InstanceId Id;

		public EntitySettings Settings;
	}

	public class EntityRemovedEvent : SceneEvent
	{
		public InstanceId Id;
	}

	public abstract class Scene
	{
		public SceneManager SceneManager { get; private set; }

		internal virtual void Initialize(SceneManager sceneManager)
		{
			SceneManager = sceneManager;

			OnInitialize();
		}

		protected abstract void OnInitialize();

		protected internal abstract Entity CreateEntity(InstanceId id, EntitySettings settings);

		protected Entity CreateEntity<TEntity, TEntitySettigns>(InstanceId id, EntitySettings settings)
			where TEntity : Entity, new()
			where TEntitySettigns : EntitySettings
		{
			return SceneManager.CreateEntity<TEntity>( id, settings );
		}

		public void ApplyEntityCreatedEvent(EntitySettings settings)
		{
			var e = SceneManager.Simulator.ReferencableAllocator.Allocate<EntityCreatedEvent>();
			e.Id = SceneManager.Simulator.InstanceIdAllocator.Allocate();
			e.Settings = settings;
			ApplyEvent( e );
		}

		public void ApplyEntityRemovedEvent(InstanceId id)
		{
			var e = SceneManager.Simulator.ReferencableAllocator.Allocate<EntityRemovedEvent>();
			e.Id = SceneManager.Simulator.InstanceIdAllocator.Allocate();
			ApplyEvent( e );
		}

		internal void ReceiveCommand(Command command)
		{
			OnCommandReceived( command );
		}

		protected virtual void OnCommandReceived(Command command)
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

		internal void ApplyEvent(Event @event)
		{
			OnEventApplied( @event );
			SceneManager.Simulator.EventBus.EnqueueEvent( @event );
			@event.Release();
		}

		protected virtual void OnEventApplied(Event @event)
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
