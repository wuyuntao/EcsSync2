using System;

namespace EcsSync2
{
	public abstract class Component : Tickable
	{
		public Entity Entity { get; private set; }
		public InstanceId Id { get; private set; }

		internal void OnInitialize(Entity entity, InstanceId id)
		{
			Entity = entity;
			Id = id;
		}

		internal abstract Snapshot OnStart(ITickContext ctx);

		internal abstract void OnFixedUpdate(ITickContext ctx, Snapshot state);

		internal abstract void OnDestroy(ITickContext ctx, Snapshot state);

		internal void ReceiveCommand(ITickContext ctx, Command command)
		{
			throw new NotImplementedException();
		}

		internal abstract void OnCommandReceived(ITickContext ctx, Snapshot state, Command command);
	}
}
