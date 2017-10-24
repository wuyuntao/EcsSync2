using System.Collections.Generic;

namespace EcsSync2
{
	public abstract class Snapshot : Message
	{
	}
	
	public class SceneSnapshot : Snapshot
	{
		public List<InstanceId> Entities = new List<InstanceId>();
	}

	public class EntitySnapshot : Snapshot
	{
		public InstanceId EntityId;

		public List<ComponentSnapshot> Components = new List<ComponentSnapshot>();
	}

	public abstract class ComponentSnapshot : Snapshot
	{
		public InstanceId ComponentId;
	}
}
