using MessagePack;

namespace EcsSync2
{
	public abstract class Command : Referencable
	{
	}

	public abstract class SceneCommand : Command
	{
	}

	public abstract class ComponentCommand : Command
	{
		[Key( 0 )]
		public uint ComponentId;
	}
}
