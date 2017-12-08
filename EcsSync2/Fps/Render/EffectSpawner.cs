using ProtoBuf;

namespace EcsSync2.Fps
{
	[ProtoContract]
	public class EffectSpawnerSnapshot : RendererSnapshot
	{
	}

	public sealed class EffectSpawner : Renderer<EffectSpawnerSnapshot>
	{
		public EventHandler<EffectSpawner, string, string> OnEffectCreated;
		public EventHandler<EffectSpawner, string, string> OnEffectDestroyed;

		protected override void OnInitialize()
		{
			OnEffectCreated = CreateEventHandler<EffectSpawner, string, string>();
			OnEffectDestroyed = CreateEventHandler<EffectSpawner, string, string>();
		}

		protected override EffectSpawnerSnapshot OnRenderStateStartedEventApplied(RenderStateStartedEvent e)
		{
			var s = base.OnRenderStateStartedEventApplied( e );
			OnEffectCreated.Invoke( this, e.OwnerId, e.StateId );
			return s;
		}

		protected override EffectSpawnerSnapshot OnRenderStateEndedEventApplied(RenderStateEndedEvent e)
		{
			var s = base.OnRenderStateEndedEventApplied( e );
			OnEffectDestroyed.Invoke( this, e.OwnerId, e.StateId );
			return s;
		}

		internal void ApplyEffectCreatedEvent(string ownerId, string stateId, bool autoDestroy)
		{
			ApplyRenderStateStartedEvent( ownerId, stateId, autoDestroy );
		}

		internal void ApplyEffectDestroyedEvent(string ownerId, string stateId)
		{
			ApplyRenderStateEndedEvent( ownerId, stateId );
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

		protected override void OnFixedUpdate()
		{
		}

		protected override void OnCommandReceived(ComponentCommand command)
		{
		}
	}
}
