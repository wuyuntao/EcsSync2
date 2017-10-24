using System;

namespace EcsSync2
{
	public class ConnectionManager : Component
	{
		internal override Snapshot OnStart(ComponentScheduler.ITickContext context)
		{
			throw new NotImplementedException();
		}

		internal override Snapshot OnFixedUpdate(ComponentScheduler.ITickContext context, Snapshot state)
		{
			throw new NotImplementedException();
		}

		internal override void OnDestroy(ComponentScheduler.ITickContext context, Snapshot state)
		{
			throw new NotImplementedException();
		}

		internal override Snapshot OnCommandReceived(ComponentScheduler.ITickContext context, Snapshot state)
		{
			throw new NotImplementedException();
		}

		internal override Snapshot OnEventApplied(ComponentScheduler.ITickContext context, Snapshot state, Event @event)
		{
			throw new NotImplementedException();
		}
	}
}
