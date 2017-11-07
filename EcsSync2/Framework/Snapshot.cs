using MessagePack;
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
		[Key( 0 )]
		public uint Id;

		[Key( 1 )]
		public EntitySettings Settings;

		[Key( 2 )]
		public List<ComponentSnapshot> Components = new List<ComponentSnapshot>();

		public override Snapshot Clone()
		{
			throw new NotImplementedException();
		}
	}

	public abstract class ComponentSnapshot : Snapshot
	{
		[Key( 0 )]
		public uint Id;
	}
}
