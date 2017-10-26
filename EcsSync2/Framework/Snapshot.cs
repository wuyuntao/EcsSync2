using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public abstract class Snapshot : Message
	{
		internal abstract bool IsApproximate(Snapshot other);

		internal abstract Snapshot Interpolate(ReferencableAllocator allocator, Snapshot other, float factor);

		internal abstract Snapshot Clone(ReferencableAllocator allocator);

		internal abstract Snapshot Extrapolate(ReferencableAllocator allocator, uint time, uint extrapolateTime);

		internal abstract Snapshot Interpolate(ReferencableAllocator allocator, uint time, Snapshot targetSnapshot, uint targetTime, uint interpolateTime);
	}

	public class SceneSnapshot : Snapshot
	{
		public List<InstanceId> Entities = new List<InstanceId>();

		internal override Snapshot Clone(ReferencableAllocator allocator)
		{
			throw new NotImplementedException();
		}

		internal override Snapshot Extrapolate(ReferencableAllocator allocator, uint time, uint extrapolateTime)
		{
			throw new NotImplementedException();
		}

		internal override Snapshot Interpolate(ReferencableAllocator allocator, Snapshot other, float factor)
		{
			throw new NotImplementedException();
		}

		internal override Snapshot Interpolate(ReferencableAllocator allocator, uint time, Snapshot targetSnapshot, uint targetTime, uint interpolateTime)
		{
			throw new NotImplementedException();
		}

		internal override bool IsApproximate(Snapshot other)
		{
			throw new NotImplementedException();
		}
	}

	public class EntitySnapshot : Snapshot
	{
		public InstanceId Id;

		public EntitySettings Settings;

		public List<ComponentSnapshot> Components = new List<ComponentSnapshot>();

		internal override Snapshot Clone(ReferencableAllocator allocator)
		{
			throw new NotImplementedException();
		}

		internal override Snapshot Extrapolate(ReferencableAllocator allocator, uint time, uint extrapolateTime)
		{
			throw new NotImplementedException();
		}

		internal override Snapshot Interpolate(ReferencableAllocator allocator, Snapshot other, float factor)
		{
			throw new NotImplementedException();
		}

		internal override Snapshot Interpolate(ReferencableAllocator allocator, uint time, Snapshot targetSnapshot, uint targetTime, uint interpolateTime)
		{
			throw new NotImplementedException();
		}

		internal override bool IsApproximate(Snapshot other)
		{
			throw new NotImplementedException();
		}
	}

	public abstract class ComponentSnapshot : Snapshot
	{
		public InstanceId Id;
	}
}
