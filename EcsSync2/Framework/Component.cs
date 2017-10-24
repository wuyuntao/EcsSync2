using System;

namespace EcsSync2
{
	public abstract class Component
	{
		public Entity Entity { get; private set; }
		public InstanceId Id { get; private set; }

		internal void OnInitialize(Entity entity, InstanceId id)
		{
			Entity = entity;
			Id = id;
		}

		internal abstract Snapshot OnStart(ComponentScheduler.ITickContext context);

		internal abstract Snapshot OnFixedUpdate(ComponentScheduler.ITickContext context, Snapshot state);

		internal abstract void OnDestroy(ComponentScheduler.ITickContext context, Snapshot state);

		internal abstract Snapshot OnCommandReceived(ComponentScheduler.ITickContext context, Snapshot state);

		internal abstract Snapshot OnEventApplied(ComponentScheduler.ITickContext context, Snapshot state, Event @event);

		internal void ReceiveCommand(ComponentScheduler.ITickContext context, Command command)
		{
			throw new NotImplementedException();
		}

		internal Snapshot GetState(ComponentScheduler.ITickContext context)
		{
			throw new NotImplementedException();
		}
	}
}
