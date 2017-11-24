using EcsSync2.Fps;
using ProtoBuf;
using System.Collections.Generic;

namespace EcsSync2
{
	public abstract class SyncFrame : Message
	{
		[ProtoMember( 1 )]
		public uint Time;
	}

	[ProtoContract]
	public class FullSyncFrame : SyncFrame
	{
		[ProtoMember( 11 )]
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

	[ProtoContract]
	public class DeltaSyncFrame : SyncFrame
	{
		[ProtoMember( 11 )]
		public List<Event> Events = new List<Event>();

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
