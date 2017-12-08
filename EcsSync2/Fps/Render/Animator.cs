using ProtoBuf;

namespace EcsSync2.Fps
{
	[ProtoContract]
	public class AnimatorSnapshot : RendererSnapshot
	{
	}

	public sealed class Animator : Renderer<AnimatorSnapshot>
	{
		public EventHandler<Animator, string, string> OnAnimationStarted;
		public EventHandler<Animator, string, string> OnAnimationEnded;

		protected override void OnInitialize()
		{
			OnAnimationStarted = CreateEventHandler<Animator, string, string>();
			OnAnimationEnded = CreateEventHandler<Animator, string, string>();
		}

		protected override AnimatorSnapshot OnRenderStateStartedEventApplied(RenderStateStartedEvent e)
		{
			var s = base.OnRenderStateStartedEventApplied( e );
			OnAnimationStarted.Invoke( this, e.OwnerId, e.StateId );
			return s;
		}

		protected override AnimatorSnapshot OnRenderStateEndedEventApplied(RenderStateEndedEvent e)
		{
			var s = base.OnRenderStateEndedEventApplied( e );
			OnAnimationEnded.Invoke( this, e.OwnerId, e.StateId );
			return s;
		}

		internal void ApplyAnimationStartedEvent(string ownerId, string stateId)
		{
			ApplyRenderStateStartedEvent( ownerId, stateId, false );
		}

		internal void ApplyAnimationEndedEvent(string ownerId, string stateId)
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
