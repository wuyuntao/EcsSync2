using System.Collections.Generic;

namespace EcsSync2
{
	public class SyncFrame : Message
	{
		public uint Time;
	}

	public class FullSyncFrame : SyncFrame
	{
		public List<EntitySnapshot> Entities;
	}

	public class DeltaSyncFrame : SyncFrame
	{
		public List<Event> Events;
	}
}
