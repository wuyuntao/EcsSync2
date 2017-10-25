using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public abstract class Scene : Tickable
	{
		public SceneManager SceneManager { get; private set; }
		public List<Player> Players { get; private set; } = new List<Player>();

		internal virtual void OnInitialize(SceneManager sceneManager)
		{
			SceneManager = sceneManager;
		}

		protected internal abstract Entity CreateEntity(InstanceId id, EntitySettings settings);

		//internal void ReceiveCommand(Component.ITickContext ctx, SceneCommand command)
		//{
		//}

		//internal abstract void OnCommandReceived(Component.ITickContext ctx, SceneCommand command);

		protected internal void ApplyEvent(ITickContext ctx, SceneEvent @event)
		{
		}

		protected virtual SceneSnapshot OnEventApplied(ITickContext ctx, SceneEvent @event)
		{
			throw new NotImplementedException();
		}

		//internal T AllocateEvent<T>()
		//	where T : SceneEvent, new()
		//{
		//	return SceneManager.Simulator.ReferencableAllocator.Allocate<T>();
		//}
	}
}
