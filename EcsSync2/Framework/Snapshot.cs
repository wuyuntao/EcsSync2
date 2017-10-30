using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public abstract class Snapshot : Message
	{
		protected internal virtual bool IsApproximate(Snapshot other)
		{
			return this == other;
		}

		protected internal virtual Snapshot Interpolate(Snapshot other, float factor)
		{
			return Clone();
		}

		public abstract Snapshot Clone();

		protected internal virtual Snapshot Extrapolate(uint time, uint extrapolateTime)
		{
			return Clone();
		}

		protected internal virtual Snapshot Interpolate(uint time, Snapshot targetSnapshot, uint targetTime, uint interpolateTime)
		{
			return Clone();
		}
	}

	public class EntitySnapshot : Snapshot
	{
		public InstanceId Id;

		public EntitySettings Settings;

		public List<ComponentSnapshot> Components = new List<ComponentSnapshot>();

		public override Snapshot Clone()
		{
			throw new NotImplementedException();
		}
	}

	public abstract class ComponentSnapshot : Snapshot
	{
		public InstanceId Id;
	}
}
