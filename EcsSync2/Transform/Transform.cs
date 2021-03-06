﻿using ProtoBuf;
using System;
using System.Diagnostics;

namespace EcsSync2
{
	public class TransformSettings : ComponentSettings
	{
		public Vector2D Position;

		public Vector2D Velocity;

		public bool IsStatic;
	}

	[ProtoContract]
	public class TransformSnapshot : ComponentSnapshot
	{
		[ProtoMember( 21 )]
		public Vector2D Position;

		[ProtoMember( 22 )]
		public Vector2D Velocity;

		protected override void OnReset()
		{
			ComponentId = 0;
			Position = Vector2D.Zero;
			Velocity = Vector2D.Zero;
		}

		public override ComponentSnapshot Clone()
		{
			var s = this.Allocate<TransformSnapshot>();
			s.ComponentId = ComponentId;
			s.Position = Position;
			s.Velocity = Velocity;
			return s;
		}

		protected internal override bool IsApproximate(ComponentSnapshot other)
		{
			if( !( other is TransformSnapshot s ) )
				return false;

			return
				IsApproximate( ComponentId, s.ComponentId ) &&
				IsApproximate( Position, s.Position ) &&
				IsApproximate( Velocity, s.Velocity );
		}

		protected internal override ComponentSnapshot Interpolate(ComponentSnapshot other, float factor)
		{
			if( !( other is TransformSnapshot t ) )
				return Clone();

			return Interpolate( this, t, factor );
		}

		static TransformSnapshot Interpolate(TransformSnapshot t1, TransformSnapshot t2, float factor)
		{
			var t = (TransformSnapshot)t1.Clone();
			if( t2.Velocity.LengthSquared() > 0 )
				t.Position = MathUtils.Lerp( t1.Position, t2.Position, factor );
			return t;
		}

		protected internal override ComponentSnapshot Interpolate(uint time, ComponentSnapshot targetSnapshot, uint targetTime, uint interpolateTime)
		{
			if( !( targetSnapshot is TransformSnapshot t ) )
				return Clone();

			var deltaTime = targetTime - interpolateTime;
			var totalTime = Math.Min( targetTime - time, Configuration.SimulationDeltaTime );
			var factor = 1 - 1f * deltaTime / totalTime;

			return Interpolate( this, t, factor );
		}

		protected internal override ComponentSnapshot Extrapolate(uint time, uint extrapolateTime)
		{
			if( Velocity.LengthSquared() <= 0 )
				return Clone();

			var t = (TransformSnapshot)Clone();
			t.Position = Position + Velocity * ( extrapolateTime - time ) / 1000f;
			return t;
		}
	}

	[ProtoContract]
	[ProtoSubType( typeof( ComponentEvent ), 3 )]
	public class TransformMovedEvent : ComponentEvent
	{
		[ProtoMember( 21 )]
		public Vector2D Position;

		protected override void OnReset()
		{
			ComponentId = 0;
			Position = Vector2D.Zero;
		}
	}

	[ProtoContract]
	[ProtoSubType( typeof( ComponentEvent ), 4 )]
	public class TransformVelocityChangedEvent : ComponentEvent
	{
		[ProtoMember( 21 )]
		public Vector2D Velocity;

		protected override void OnReset()
		{
			ComponentId = 0;
			Velocity = Vector2D.Zero;
		}
	}

	public class Transform : Component
	{
		public EventHandler<Transform> OnMoved;

		protected override void OnInitialize()
		{
			OnMoved = CreateEventHandler<Transform>();
		}

		protected override void OnCommandReceived(ComponentCommand command)
		{
			throw new NotSupportedException( command.ToString() );
		}

		protected override ComponentSnapshot OnEventApplied(ComponentEvent @event)
		{
			switch( @event )
			{
				case TransformMovedEvent e:
					var s1 = (TransformSnapshot)State.Clone();
					//if( Entity is Character c1 && !c1.IsLocalCharacter )
					//	Entity.SceneManager.Simulator.Context.Log( $"{this} received {nameof( TransformMovedEvent )} {e.Position} <- {s1.Position}" );
					s1.Position = e.Position;
					OnMoved.Invoke( this );
					return s1;

				case TransformVelocityChangedEvent e:
					var s2 = (TransformSnapshot)State.Clone();
					//if( Entity is Character c2 && !c2.IsLocalCharacter )
					//	Entity.SceneManager.Simulator.Context.Log( $"received {nameof( TransformVelocityChangedEvent )} {e.Velocity} <- {s2.Velocity}" );
					s2.Velocity = e.Velocity;
					return s2;

				default:
					throw new NotSupportedException( @event.ToString() );
			}
		}

		protected override void OnFixedUpdate()
		{
		}

		protected override void OnSnapshotRecovered(ComponentSnapshot state)
		{
		}

		protected internal override ComponentSnapshot CreateSnapshot()
		{
			return CreateSnapshot<TransformSnapshot>();
		}

		public void ApplyTransformMovedEvent(Vector2D offset)
		{
			Debug.Assert( offset.IsValid() );

			var s = (TransformSnapshot)State;
			var e = CreateEvent<TransformMovedEvent>();
			e.Position = s.Position + offset;
			ApplyEvent( e );
		}

		public void ApplyTransformVelocityChangedEvent(Vector2D velocity)
		{
			Debug.Assert( velocity.IsValid() );

			var s = (TransformSnapshot)State;
			if( s.Velocity == velocity )
				return;

			var e = CreateEvent<TransformVelocityChangedEvent>();
			e.Velocity = velocity;
			ApplyEvent( e );
		}

		protected override void OnStart()
		{
		}

		protected override void OnDestroy()
		{
		}

		TransformSnapshot TheState => (TransformSnapshot)State;

		public Vector2D Position => TheState.Position;

		public Vector2D Velocity => TheState.Velocity;
	}
}
