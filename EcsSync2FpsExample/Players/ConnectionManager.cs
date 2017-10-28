using System;

namespace EcsSync2.FpsExample
{
	public class ConnectionManager : Component
	{
		protected override void OnCommandReceived(Command command)
		{
			throw new NotImplementedException();
		}

		protected override Snapshot OnEventApplied(Event @event)
        {
			throw new NotImplementedException();
		}

		protected override void OnFixedUpdate()
		{
			throw new NotImplementedException();
		}

		protected override void OnSnapshotRecovered(Snapshot state)
		{
			throw new NotImplementedException();
		}

		protected override Snapshot OnStart()
		{
			throw new NotImplementedException();
		}
	}
}
