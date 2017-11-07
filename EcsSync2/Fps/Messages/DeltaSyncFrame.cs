using System;
using MessagePack;
using System.Collections.Generic;

namespace EcsSync2.Fps
{
	[MessagePackObject]
	public class DeltaSyncFrameMessage : IMessage
	{
		[Key( 0 )]
		public uint Time;

		[Key( 1 )]
		public List<IEventUnion> Events;

		public static DeltaSyncFrameMessage FromDeltaSyncFrame(DeltaSyncFrame frame)
		{
			var m = new DeltaSyncFrameMessage()
			{
				Time = frame.Time,
				Events = new List<IEventUnion>(),
			};

			foreach( var c in frame.Events )
				m.Events.Add( (IEventUnion)c );

			return m;
		}

		public DeltaSyncFrame ToDeltaSyncFrame(Simulator simulator)
		{
			var frame = simulator.ReferencableAllocator.Allocate<DeltaSyncFrame>();
			frame.Time = Time;
			foreach( var c in Events )
				frame.Events.Add( (Event)c );
			return frame;
		}
	}

	[MessagePackObject]
	public class EntityCreatedEventMessage : IEventUnion
	{
		[Key( 0 )]
		public uint Id;

		[Key( 1 )]
		public IEntitySettingsUnion Settings;
	}

	[MessagePackObject]
	public class EntityRemovedEventMessage : IEventUnion
	{
		[Key( 0 )]
		public uint Id;
	}

	[Union( 0, typeof( EntityCreatedEventMessage ) )]
	[Union( 1, typeof( EntityRemovedEventMessage ) )]
	[Union( 2, typeof( InputChangedEvent ) )]
	[Union( 3, typeof( PlayerConnectedEvent ) )]
	[Union( 4, typeof( TransformMovedEvent ) )]
	[Union( 5, typeof( TransformVelocityChangedEvent ) )]
	public interface IEventUnion
	{
	}
}
