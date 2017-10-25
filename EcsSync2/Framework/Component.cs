using System;

namespace EcsSync2
{
	public abstract class Component
	{
		public interface ITickContext
		{
			uint Time { get; }

			uint DeltaTime { get; }
		}

		public Entity Entity { get; private set; }
		public InstanceId Id { get; private set; }

		internal void OnInitialize(Entity entity, InstanceId id)
		{
			Entity = entity;
			Id = id;
		}

		internal abstract Snapshot OnStart(ITickContext ctx);

		internal abstract Snapshot OnFixedUpdate(ITickContext ctx, Snapshot state);

		internal abstract void OnDestroy(ITickContext ctx, Snapshot state);

		internal void ReceiveCommand(ITickContext ctx, Command command)
		{
			throw new NotImplementedException();
		}

		internal abstract Snapshot OnCommandReceived(ITickContext ctx, Snapshot state);

		internal void ApplyEvent(ITickContext ctx, Event e)
		{
			throw new NotImplementedException();
		}

		internal abstract Snapshot OnEventApplied(ITickContext ctx, Snapshot state, Event @event);

		internal Snapshot GetState(ITickContext ctx)
		{
			throw new NotImplementedException();
		}

		internal abstract void OnSnapshotRecovered(ITickContext ctx, ComponentSnapshot cs);

		internal void RecoverSnapshot(ITickContext ctx, Snapshot state)
		{
			throw new NotImplementedException();
		}
	}
}
