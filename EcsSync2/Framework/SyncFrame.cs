using System.Collections.Generic;

namespace EcsSync2
{
	public class SyncFrame : Referencable
	{
		public uint Time;
	}

	public class FullSyncFrame : SyncFrame
	{
		public List<EntitySnapshot> Entities = new List<EntitySnapshot>();

		protected override void Reset()
		{
			foreach( var e in Entities )
				e.Release();

			Entities.Clear();

			base.Reset();
		}
	}

	public class DeltaSyncFrame : SyncFrame
	{
		public List<Event> Events = new List<Event>();

		protected override void Reset()
		{
			foreach( var e in Events )
				e.Release();

			Events.Clear();

			base.Reset();
		}
	}
}
