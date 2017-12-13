using ProtoBuf;
using System;

namespace EcsSync2.Fps
{
	[ProtoContract]
	public class JumpCommand : ComponentCommand
	{
		protected override void OnReset()
		{
			ComponentId = 0;
		}
	}

	[ProtoContract]
	public class JumperSnapshot : ComponentSnapshot
	{
		[ProtoMember( 21 )]
		public uint JumpStopTime;

		public override ComponentSnapshot Clone()
		{
			var s = this.Allocate<JumperSnapshot>();
			s.ComponentId = ComponentId;
			s.JumpStopTime = JumpStopTime;
			return s;
		}

		protected internal override bool IsApproximate(ComponentSnapshot other)
		{
			if( !( other is JumperSnapshot s ) )
				return false;

			return
				IsApproximate( ComponentId, s.ComponentId ) &&
				IsApproximate( JumpStopTime, s.JumpStopTime );
		}

		protected override void OnReset()
		{
			ComponentId = 0;
			JumpStopTime = 0;
		}
	}

	[ProtoContract]
	public class JumpStartedEvent : ComponentEvent
	{
		[ProtoMember( 1 )]
		public uint StopTime;

		protected override void OnReset()
		{
			ComponentId = 0;
			StopTime = 0;
		}
	}

	[ProtoContract]
	public class JumpStoppedEvent : ComponentEvent
	{
		protected override void OnReset()
		{
			ComponentId = 0;
		}
	}

	public class Jumper : Component
	{
		const uint JumpDuration = 917;

		public EventHandler<Jumper> OnJumpStarted;
		public EventHandler<Jumper> OnJumpStopped;

		protected override void OnInitialize()
		{
			OnJumpStarted = CreateEventHandler<Jumper>();
			OnJumpStopped = CreateEventHandler<Jumper>();
		}

		protected internal override ComponentSnapshot CreateSnapshot()
		{
			return CreateSnapshot<JumperSnapshot>();
		}

		protected override void OnCommandReceived(ComponentCommand command)
		{
			switch( command )
			{
				case JumpCommand c:
					if( !IsJumping )
						ApplyJumpStartedEvent();
					break;

				default:
					throw new NotSupportedException( command.ToString() );
			}
		}

		void ApplyJumpStartedEvent()
		{
			var e = CreateEvent<JumpStartedEvent>();
			e.StopTime = Time + JumpDuration;
			ApplyEvent( e );
		}

		protected override void OnDestroy()
		{
		}

		protected override ComponentSnapshot OnEventApplied(ComponentEvent @event)
		{
			switch( @event )
			{
				case JumpStartedEvent e:
					return OnJumpStartedEvent( e );

				case JumpStoppedEvent e:
					return OnJumpStoppedEvent( e );

				default:
					throw new NotSupportedException( @event.ToString() );
			}
		}

		JumperSnapshot OnJumpStartedEvent(JumpStartedEvent e)
		{
			var s = (JumperSnapshot)State.Clone();
			s.JumpStopTime = e.StopTime;
			OnJumpStarted.Invoke( this );
			return s;
		}

		JumperSnapshot OnJumpStoppedEvent(JumpStoppedEvent e)
		{
			var s = (JumperSnapshot)State.Clone();
			s.JumpStopTime = 0;
			OnJumpStopped.Invoke( this );
			return s;
		}

		protected override void OnFixedUpdate()
		{
		}

		protected override void OnSnapshotRecovered(ComponentSnapshot state)
		{
		}

		protected override void OnStart()
		{
		}

		JumperSnapshot TheState => (JumperSnapshot)State;

		public bool IsJumping => TheState.JumpStopTime > Time;
	}
}