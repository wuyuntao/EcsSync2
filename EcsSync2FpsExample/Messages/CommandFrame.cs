using MessagePack;
using System.Collections.Generic;

namespace EcsSync2.FpsExample
{
	[MessagePackObject]
	public class CommandFrameMessage
	{
		[Key( 0 )]
		public uint Time;

		[Key( 1 )]
		public List<ICommandUnion> Commands;
	}

	[Union( 0, typeof( CreateEntityCommandMessage ) )]
	[Union( 1, typeof( RemoveEntityCommandMessage ) )]
	[Union( 2, typeof( PlayerConnectCommand ) )]
	[Union( 3, typeof( MoveCharacterCommand ) )]
	public interface ICommandUnion
	{
	}

	[MessagePackObject]
	public class CreateEntityCommandMessage : ICommandUnion
	{
		[Key( 0 )]
		public IEntitySettingsUnion Settings;
	}

	[MessagePackObject]
	public class RemoveEntityCommandMessage : ICommandUnion
	{
		[Key( 0 )]
		public uint Id;
	}

	[Union( 0, typeof( CharacterSettings ) )]
	[Union( 1, typeof( GameManagerSettings ) )]
	[Union( 2, typeof( ItemSettings ) )]
	[Union( 3, typeof( PlayerSettings ) )]
	[Union( 4, typeof( SceneElementSettings ) )]
	public interface IEntitySettingsUnion
	{
	}
}
