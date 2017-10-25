using System;

namespace EcsSync2
{
	public class ConnectionManager : Component
	{
		internal override Snapshot OnStart(ITickContext context)
		{
			throw new NotImplementedException();
		}

		internal override void OnDestroy(ITickContext context, Snapshot state)
		{
			throw new NotImplementedException();
		}

		protected override Snapshot OnEventApplied(ITickContext ctx, Snapshot state, Event @event)
		{
			throw new NotImplementedException();
		}

		protected override void OnSnapshotRecovered(ITickContext ctx, Snapshot cs)
		{
			throw new NotImplementedException();
		}

		internal override void OnFixedUpdate(ITickContext ctx, Snapshot state)
		{
			throw new NotImplementedException();
		}

		internal override void OnCommandReceived(ITickContext ctx, Snapshot state, Command command)
		{
			throw new NotImplementedException();
		}
	}
}
