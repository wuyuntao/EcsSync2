namespace EcsSync2
{
	public abstract class Event : Message
	{
	}

	public abstract class SceneEvent : Event
	{
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

	public class ComponentEvent : Event
	{
		public InstanceId ComponentId;
	}
}
