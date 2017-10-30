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
			where TEntity : Entity
			where TEntitySettigns : EntitySettings
		{
			throw new NotSupportedException();
		}

		protected void ApplyEntityCreatedEvent(EntitySettings settings)
		{
		}
		protected void ApplyEntityRemovedEvent(InstanceId id)
		{
		}

		protected virtual void OnCommandReceived(Command command)
		{
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

		internal void ReceiveCommand(Command command)
		{
			OnCommandReceived( command );
		}

		internal void ApplyEvent(Event @event)
		{
			throw new NotImplementedException();
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
