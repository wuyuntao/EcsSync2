using MessagePack;
using System;

namespace EcsSync2.FpsExample
{
	[MessagePackObject]
	public class ProcessControllerSnapshot : ComponentSnapshot, IComponentSnapshotUnion
	{
		public override Snapshot Clone()
		{
			return this.Allocate<ProcessControllerSnapshot>();
		}
	}

	public class ProcessController : Component
	{
		protected override void OnInitialize()
		{
		}

		protected override void OnStart()
		{
		}

		protected override void OnDestroy()
		{
		}

		protected override void OnSnapshotRecovered(Snapshot state)
		{
		}

		protected override Snapshot CreateSnapshot()
		{
			return Entity.SceneManager.Simulator.ReferencableAllocator.Allocate<ProcessControllerSnapshot>();
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
	}
}
