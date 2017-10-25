using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public abstract class Snapshot : Message
	{
		internal abstract bool IsApproximate(Snapshot other);

		internal abstract Snapshot Interpolate(Snapshot other, float factor);
	}

	public class SceneSnapshot : Snapshot
	{
		public List<InstanceId> Entities = new List<InstanceId>();

		internal override Snapshot Interpolate(Snapshot other, float factor)
		{
			throw new NotImplementedException();
		}

		internal override bool IsApproximate(Snapshot other)
		{
			throw new System.NotImplementedException();
		}
	}

	public class EntitySnapshot : Snapshot
	{
		public InstanceId Id;

		public EntitySettings Settings;

		public List<ComponentSnapshot> Components = new List<ComponentSnapshot>();

		internal override Snapshot Interpolate(Snapshot other, float factor)
		{
			throw new NotImplementedException();
		}

		internal override bool IsApproximate(Snapshot other)
		{
			throw new System.NotImplementedException();
		}
	}

	public abstract class ComponentSnapshot : Snapshot
	{
		public InstanceId Id;
	}
}
