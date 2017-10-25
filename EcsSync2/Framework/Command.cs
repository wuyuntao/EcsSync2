namespace EcsSync2
{
	public abstract class Command : Message
	{
		public InstanceId Receiver;
	}

	public class MoveCharacterCommand : Command
	{
		public float[] Direction;

		public float Magnitude;
	}
}
