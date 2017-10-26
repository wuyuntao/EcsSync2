using System;

namespace EcsSync2
{
	public class ComponentSettings
	{
	}

	public abstract class Component : Tickable
	{
		public Entity Entity { get; private set; }
		public InstanceId Id { get; private set; }
		public ComponentSettings Settings { get; private set; }

		internal void OnInitialize(Entity entity, InstanceId id, ComponentSettings settings)
		{
			Entity = entity;
			Id = id;
			Settings = settings;
		}

		internal void ReceiveCommand(Command command)
		{
		}

		protected abstract void OnCommandReceived(Command command);
	}
}
