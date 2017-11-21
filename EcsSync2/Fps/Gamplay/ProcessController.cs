using MessagePack;
using System;

namespace EcsSync2.Fps
{
	[MessagePackObject]
	public class ProcessControllerSnapshot : ComponentSnapshot, IComponentSnapshot
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

		protected override void OnSnapshotRecovered(ComponentSnapshot state)
		{
		}

		protected internal override ComponentSnapshot CreateSnapshot()
		{
			return CreateSnapshot<ProcessControllerSnapshot>();
		}

		protected override void OnFixedUpdate()
		{
		}

		protected override void OnCommandReceived(ComponentCommand command)
		{
			throw new NotSupportedException( command.ToString() );
		}

		protected override ComponentSnapshot OnEventApplied(ComponentEvent @event)
		{
			throw new NotSupportedException( @event.ToString() );
		}
	}
}
