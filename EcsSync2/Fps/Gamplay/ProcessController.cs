using ProtoBuf;
using System;

namespace EcsSync2.Fps
{
	[ProtoContract]
	public class ProcessControllerSnapshot : ComponentSnapshot
	{
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
