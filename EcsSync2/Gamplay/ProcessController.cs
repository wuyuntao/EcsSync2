using System;

namespace EcsSync2
{
	public class ProcessController : Component
	{
		internal override Snapshot OnStart(ITickContext ctx)
		{
			throw new NotImplementedException();
		}

		internal override Snapshot OnFixedUpdate(ITickContext ctx, Snapshot state)
		{
			throw new NotImplementedException();
		}

		internal override void OnDestroy(ITickContext ctx, Snapshot state)
		{
			throw new NotImplementedException();
		}

		internal override Snapshot OnCommandReceived(ITickContext ctx, Snapshot state)
		{
			throw new NotImplementedException();
		}

		internal override Snapshot OnEventApplied(ITickContext ctx, Snapshot state, Event @event)
		{
			throw new NotImplementedException();
		}

		internal override void OnSnapshotRecovered(ITickContext ctx, ComponentSnapshot cs)
		{
			throw new NotImplementedException();
		}
	}
}
