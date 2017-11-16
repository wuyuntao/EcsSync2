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
		[Key( 10 )]
		public uint Id;

		[Key( 11 )]
		public IEntitySettings Settings;

		[Key( 12 )]
		public List<ComponentSnapshot> Components = new List<ComponentSnapshot>();

		public override Snapshot Clone()
		{
			throw new NotImplementedException();
		}

		protected override void Reset()
		{
			foreach( var c in Components )
				c.Release();

			Components.Clear();

			base.Reset();
		}
	}

	public abstract class ComponentSnapshot : Snapshot
	{
		[Key( 10 )]
		public uint ComponentId;
	}
}
