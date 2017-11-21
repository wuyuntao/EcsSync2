using EcsSync2.Fps;
using MessagePack;
using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public abstract class Snapshot : Referencable
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

	[MessagePackObject]
	public class EntitySnapshot : Snapshot
	{
		[Key( 10 )]
		public uint Id;

		[Key( 11 )]
		public IEntitySettings Settings;

		[Key( 12 )]
		public List<IComponentSnapshot> Components = new List<IComponentSnapshot>();

		public override Snapshot Clone()
		{
			throw new NotImplementedException();
		}

		protected override void Reset()
		{
			foreach( ComponentSnapshot c in Components )
				c.Release();

			Components.Clear();

			base.Reset();
		}
	}

	[Union( 0, typeof( CharacterMotionControllerSnapshot ) )]
	[Union( 1, typeof( TransformSnapshot ) )]
	[Union( 2, typeof( ProcessControllerSnapshot ) )]
	[Union( 3, typeof( ConnectingSnapshot ) )]
	[Union( 4, typeof( ConnectedSnapshot ) )]
	[Union( 5, typeof( DisconnectedSnapshot ) )]
	public interface IComponentSnapshot
	{
	}

	public abstract class ComponentSnapshot : Snapshot, IComponentSnapshot
	{
		[Key( 10 )]
		public uint ComponentId;
	}
}
