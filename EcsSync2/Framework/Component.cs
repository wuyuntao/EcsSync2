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

		internal void ReceiveCommand(Command command)
		{
		}

		protected abstract void OnCommandReceived(Command command);
	}
}
