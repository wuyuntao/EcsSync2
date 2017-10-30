using System;

namespace EcsSync2.FpsExample
{
	public class ProcessController : Component
	{
		protected override void OnSnapshotRecovered(Snapshot state)
		{
		}

		protected override Snapshot OnFixedStart()
		{
			throw new NotImplementedException();
		}

		protected override void OnFixedUpdate()
		{
		}

		protected override void OnCommandReceived(Command command)
		{
			throw new NotSupportedException( command.ToString() );
		}

		protected override Snapshot OnEventApplied(Event @event)
		{
			throw new NotImplementedException();
		}

		protected override void OnInitialize()
		{
			throw new NotImplementedException();
		}

		protected override void OnStart()
		{
			throw new NotImplementedException();
		}

		protected override void OnDestroy()
		{
			throw new NotImplementedException();
		}
	}
}
