using ProtoBuf;

namespace EcsSync2.Fps
{
	[ProtoContract]
	public class PawnSnapshot : RendererSnapshot
	{
	}

	public abstract class Pawn : Renderer<PawnSnapshot>
	{
		public EventHandler<Pawn, string, string> OnPawnStateStarted;
		public EventHandler<Pawn, string, string> OnPawnStateEnded;

		protected override void OnInitialize()
		{
			OnPawnStateStarted = CreateEventHandler<Pawn, string, string>();
			OnPawnStateEnded = CreateEventHandler<Pawn, string, string>();
		}

		protected override PawnSnapshot OnRenderStateStartedEventApplied(RenderStateStartedEvent e)
		{
			var s = base.OnRenderStateStartedEventApplied( e );
			OnPawnStateStarted.Invoke( this, e.OwnerId, e.StateId );
			return s;
		}

		protected override PawnSnapshot OnRenderStateEndedEventApplied(RenderStateEndedEvent e)
		{
			var s = base.OnRenderStateEndedEventApplied( e );
			OnPawnStateEnded.Invoke( this, e.OwnerId, e.StateId );
			return s;
		}

		internal void ApplyPawnStateStartedEvent(string ownerId, string stateId, bool isInstantaneous)
		{
			ApplyRenderStateStartedEvent( ownerId, stateId, isInstantaneous );
		}

		internal void ApplyPawnStateEndedEvent(string ownerId, string stateId)
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
