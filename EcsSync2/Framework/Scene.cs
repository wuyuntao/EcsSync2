using System;

namespace EcsSync2
{
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

		internal void ApplyEvent(Event @event)
		{
			throw new NotImplementedException();
		}

		internal void OnEventApplied(Event @event)
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
