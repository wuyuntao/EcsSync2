namespace EcsSync2
{
	public abstract class Command : Message
	{
		public InstanceId Receiver;

		public Message Args;
	}
}
