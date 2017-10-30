namespace EcsSync2
{
	public abstract class Command : Message
	{
	}

	public abstract class SceneCommand : Command
	{
	}

	public abstract class ComponentCommand : Command
	{
		public InstanceId Receiver;
	}
}
