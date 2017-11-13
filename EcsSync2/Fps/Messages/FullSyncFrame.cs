using MessagePack;
using System.Collections.Generic;
using System;
using System.Linq;

namespace EcsSync2.Fps
{
	[MessagePackObject]
	public class FullSyncFrameMessage : IMessage
	{
		[Key( 0 )]
		public uint Time;

		[Key( 1 )]
		public List<EntitySnapshotMessage> Entities;

		public static FullSyncFrameMessage FromFullSyncFrame(FullSyncFrame frame)
		{
			var m = new FullSyncFrameMessage()
			{
				Time = frame.Time,
				Entities = new List<EntitySnapshotMessage>(),
			};

			foreach( var e in frame.Entities )
			{
				m.Entities.Add( new EntitySnapshotMessage()
				{
					Id = e.Id,
					Settings = (IEntitySettingsUnion)e.Settings,
					Components = e.Components.Cast<IComponentSnapshotUnion>().ToList(),
				} );
			}

			return m;
		}

		public FullSyncFrame ToFullSyncFrame(Simulator simulator)
		{
			var frame = simulator.ReferencableAllocator.Allocate<FullSyncFrame>();
			frame.Time = Time;
			foreach( var c in Entities )
			{
				var e = frame.Allocate<EntitySnapshot>();
				e.Id = c.Id;
				e.Settings = c.Settings;
				e.Components = e.Components.Cast<ComponentSnapshot>().ToList();

				frame.Entities.Add( e );
			}

			return frame;
		}
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
