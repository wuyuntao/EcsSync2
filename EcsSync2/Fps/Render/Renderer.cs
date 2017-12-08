using ProtoBuf;
using System;
using System.Collections.Generic;

namespace EcsSync2.Fps
{
	[ProtoContract]
	public class RenderState : SerializableReferencable
	{
		[ProtoMember( 1 )]
		public string OwnerId;

		[ProtoMember( 2 )]
		public string StateId;

		protected override void OnReset()
		{
			OwnerId = null;
			StateId = null;

			base.OnReset();
		}
	}

	[ProtoContract]
	[ProtoInclude( 1, typeof( AnimatorSnapshot ) )]
	public class RendererSnapshot : ComponentSnapshot
	{
		public List<RenderState> States = new List<RenderState>();

		protected override void OnAllocate()
		{
			base.OnAllocate();

			foreach( var state in States )
				this.Allocate( state );
		}

		protected override void OnReset()
		{
			foreach( var state in States )
				state.Release();
			States.Clear();

			base.OnReset();
		}

		public override ComponentSnapshot Clone()
		{
			var s = this.Allocate<RendererSnapshot>();
			foreach( var state in States )
			{
				var cs = this.Allocate<RenderState>();
				cs.OwnerId = state.OwnerId;
				cs.StateId = state.StateId;

				s.States.Add( cs );
			}
			return s;
		}

		internal RenderState FindState(string ownerId)
		{
			foreach( var s in States )
			{
				if( s.OwnerId == ownerId )
					return s;
			}
			return null;
		}

		internal RenderState FindState(string ownerId, string stateId)
		{
			foreach( var s in States )
			{
				if( s.OwnerId == ownerId && s.StateId == stateId )
					return s;
			}
			return null;
		}

		internal void AddState(string ownerId, string stateId)
		{
			var state = this.Allocate<RenderState>();
			state.OwnerId = ownerId;
			state.StateId = stateId;
			States.Add( state );
		}

		internal void RemoveState(string ownerId, string stateId)
		{
			States.RemoveAll( s => s.OwnerId == ownerId && s.StateId == stateId );
		}
	}

	[ProtoContract]
	[ProtoInclude( 1, typeof( RenderStateStartedEvent ) )]
	[ProtoInclude( 2, typeof( RenderStateEndedEvent ) )]
	public class RenderStateEvent : ComponentEvent
	{
		[ProtoMember( 1 )]
		public string OwnerId;

		[ProtoMember( 2 )]
		public string StateId;

		protected override void OnReset()
		{
			ComponentId = 0;

			OwnerId = null;
			StateId = null;
		}
	}

	[ProtoContract]
	public class RenderStateStartedEvent : RenderStateEvent
	{
		[ProtoMember( 1 )]
		public bool IsInstantaneous;
	}

	[ProtoContract]
	public class RenderStateEndedEvent : RenderStateEvent
	{
	}

	public abstract class Renderer<TSnapshot> : Component
		where TSnapshot : RendererSnapshot, new()
	{
		protected internal override ComponentSnapshot CreateSnapshot()
		{
			return CreateSnapshot<TSnapshot>();
		}

		protected override ComponentSnapshot OnEventApplied(ComponentEvent @event)
		{
			switch( @event )
			{
				case RenderStateStartedEvent e:
					return OnRenderStateStartedEventApplied( e );

				case RenderStateEndedEvent e:
					return OnRenderStateEndedEventApplied( e );

				default:
					throw new NotSupportedException( @event.ToString() );
			}
		}

		protected virtual TSnapshot OnRenderStateStartedEventApplied(RenderStateStartedEvent e)
		{
			TSnapshot snapshot = null;
			if( !e.IsInstantaneous )
			{
				snapshot = (TSnapshot)State.Clone();
				var state = snapshot.FindState( e.OwnerId );
				if( state != null )
				{
					state.StateId = e.StateId;
				}
				else
				{
					snapshot.AddState( e.OwnerId, e.StateId );
					state.Release();
				}
			}
			return snapshot;
		}

		protected virtual TSnapshot OnRenderStateEndedEventApplied(RenderStateEndedEvent e)
		{
			var snapshot = (TSnapshot)State;
			var state = snapshot.FindState( e.OwnerId, e.StateId );
			if( state == null )
				return null;

			snapshot = (TSnapshot)State.Clone();
			snapshot.RemoveState( e.OwnerId, e.StateId );
			return snapshot;
		}

		protected virtual void ApplyRenderStateStartedEvent(string ownerId, string stateId, bool isInstantaneous)
		{
			var e = CreateEvent<RenderStateStartedEvent>();
			e.OwnerId = ownerId;
			e.StateId = stateId;
			e.IsInstantaneous = isInstantaneous;

			ApplyEvent( e );
		}

		protected virtual void ApplyRenderStateEndedEvent(string ownerId, string stateId)
		{
			var e = CreateEvent<RenderStateEndedEvent>();
			e.OwnerId = ownerId;
			e.StateId = stateId;

			ApplyEvent( e );
		}
	}
}
