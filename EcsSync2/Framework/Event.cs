namespace EcsSync2
{
	public abstract class Event : Message
	{
	}

	public class SceneEvent : Event
	{
	}

	public class ComponentEvent : Event
	{
		public InstanceId ComponentId;
	}
}
