using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public abstract class Snapshot : Message
	{
		protected internal abstract bool IsApproximate(Snapshot other);

        protected internal abstract Snapshot Interpolate(Snapshot other, float factor);

        public abstract Snapshot Clone();

        protected internal abstract Snapshot Extrapolate(uint time, uint extrapolateTime);

        protected internal abstract Snapshot Interpolate(uint time, Snapshot targetSnapshot, uint targetTime, uint interpolateTime);
	}

	public class SceneSnapshot : Snapshot
	{
		public List<InstanceId> Entities = new List<InstanceId>();

        public override Snapshot Clone()
		{
			throw new NotImplementedException();
		}

        protected internal override Snapshot Extrapolate(uint time, uint extrapolateTime)
        {
			throw new NotImplementedException();
		}

        protected internal override Snapshot Interpolate(Snapshot other, float factor)
        {
			throw new NotImplementedException();
		}

        protected internal override Snapshot Interpolate(uint time, Snapshot targetSnapshot, uint targetTime, uint interpolateTime)
        {
			throw new NotImplementedException();
		}

        protected internal override bool IsApproximate(Snapshot other)
		{
			throw new NotImplementedException();
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

        protected internal override Snapshot Extrapolate(uint time, uint extrapolateTime)
        {
			throw new NotImplementedException();
		}

        protected internal override Snapshot Interpolate(Snapshot other, float factor)
        {
			throw new NotImplementedException();
		}

        protected internal override Snapshot Interpolate(uint time, Snapshot targetSnapshot, uint targetTime, uint interpolateTime)
        {
			throw new NotImplementedException();
		}

        protected internal override bool IsApproximate(Snapshot other)
		{
			throw new NotImplementedException();
		}
	}

	public abstract class ComponentSnapshot : Snapshot
	{
		public InstanceId Id;
	}
}
