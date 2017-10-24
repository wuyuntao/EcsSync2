namespace EcsSync2
{
	public abstract class Scene
	{
		internal abstract SceneSnapshot OnStart(ComponentScheduler.ITickContext context);

		internal abstract SceneSnapshot OnFixedUpdate(ComponentScheduler.ITickContext context, SceneSnapshot state);

		internal abstract SceneSnapshot OnEventApplied(ComponentScheduler.ITickContext context, Event @event);
	}
}
