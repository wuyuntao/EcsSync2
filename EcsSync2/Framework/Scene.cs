using System.Collections.Generic;

namespace EcsSync2
{
	public abstract class Scene
	{
		public IList<Player> Players { get; set; }

		internal abstract SceneSnapshot OnStart(Component.ITickContext ctx);

		internal abstract SceneSnapshot OnFixedUpdate(Component.ITickContext ctx, SceneSnapshot state);

		internal abstract SceneSnapshot OnEventApplied(Component.ITickContext ctx, Event @event);
	}
}
