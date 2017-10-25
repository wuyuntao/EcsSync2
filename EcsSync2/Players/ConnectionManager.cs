using System;

namespace EcsSync2
{
	public class ConnectionManager : Component
	{
		internal override Snapshot OnStart(ITickContext context)
		{
			throw new NotImplementedException();
		}

		internal override Snapshot OnFixedUpdate(ITickContext context, Snapshot state)
		{
			throw new NotImplementedException();
		}

		internal override void OnDestroy(ITickContext context, Snapshot state)
		{
			throw new NotImplementedException();
		}

		internal override Snapshot OnCommandReceived(ITickContext context, Snapshot state)
		{
			throw new NotImplementedException();
		}

		internal override Snapshot OnEventApplied(ITickContext context, Snapshot state, Event @event)
		{
			throw new NotImplementedException();
		}

		internal override void OnSnapshotRecovered(ITickContext ctx, ComponentSnapshot cs)
		{
			throw new NotImplementedException();
		}
	}
}
