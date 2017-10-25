namespace EcsSync2
{
	public abstract class Scene
	{
		internal abstract SceneSnapshot OnStart(Component.ITickContext ctx);

		internal abstract SceneSnapshot OnFixedUpdate(Component.ITickContext ctx, SceneSnapshot state);

		internal abstract SceneSnapshot OnEventApplied(Component.ITickContext ctx, Event @event);
	}
}
