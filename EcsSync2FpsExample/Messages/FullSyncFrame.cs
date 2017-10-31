using MessagePack;
using System.Collections.Generic;

namespace EcsSync2.FpsExample
{
	[MessagePackObject]
	public class FullSyncFrameMessage
	{
		[Key( 0 )]
		public uint Time;

		[Key( 1 )]
		public List<EntitySnapshotMessage> Entities;
	}

	[MessagePackObject]
	public class EntitySnapshotMessage
	{
		[Key( 0 )]
		public uint Id;

		[Key( 1 )]
		public IEntitySettingsUnion Settings;

		[Key( 2 )]
		public List<IComponentSnapshotUnion> Components;
	}

	[Union( 0, typeof( CharacterMotionControllerSnapshot ) )]
	[Union( 1, typeof( TransformSnapshot ) )]
	[Union( 2, typeof( ProcessControllerSnapshot ) )]
	[Union( 3, typeof( ConnectingSnapshot ) )]
	[Union( 4, typeof( ConnectedSnapshot ) )]
	[Union( 5, typeof( DisconnectedSnapshot ) )]
	public interface IComponentSnapshotUnion
	{
	}
}
