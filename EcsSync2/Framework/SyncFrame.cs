using EcsSync2.Fps;
using MessagePack;
using System.Collections.Generic;

namespace EcsSync2
{
	public abstract class SyncFrame : Referencable, IMessage
	{
		[Key( 0 )]
		public uint Time;
	}

	[MessagePackObject]
	public class FullSyncFrame : SyncFrame
	{
		[Key( 10 )]
		public List<EntitySnapshot> Entities = new List<EntitySnapshot>();

		protected override void OnAllocate()
		{
			base.OnAllocate();

			foreach( var e in Entities )
				this.Allocate( e );
		}

		protected override void OnReset()
		{
			foreach( var e in Entities )
				e.Release();

			Entities.Clear();

			base.OnReset();
		}
	}

	[MessagePackObject]
	public class DeltaSyncFrame : SyncFrame
	{
		[Key( 10 )]
		public List<IEvent> Events = new List<IEvent>();

		protected override void OnAllocate()
		{
			base.OnAllocate();

			foreach( var e in Events )
				this.Allocate( e );
		}

		protected override void OnReset()
		{
			foreach( Event e in Events )
				e.Release();

			Events.Clear();

			base.OnReset();
		}
	}
}
