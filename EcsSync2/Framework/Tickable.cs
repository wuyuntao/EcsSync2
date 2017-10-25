using System;

namespace EcsSync2
{
	public abstract class Tickable
	{
		public interface ITickContext
		{
			uint Time { get; }

			uint DeltaTime { get; }
		}

		protected internal Snapshot GetSnapshot(ITickContext ctx)
		{
			throw new NotImplementedException();
		}

		protected internal void RecoverSnapshot(ITickContext ctx, Snapshot state)
		{
			throw new NotImplementedException();
		}

		protected abstract void OnSnapshotRecovered(ITickContext ctx, Snapshot state);

		protected internal void ApplyEvent(ITickContext ctx, Event @event)
		{
			throw new NotImplementedException();
		}

		
	}
}
