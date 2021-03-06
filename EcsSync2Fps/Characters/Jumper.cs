﻿using ProtoBuf;
using System;

namespace EcsSync2.Fps
{
	[ProtoContract]
	[ProtoSubType( typeof( ComponentCommand ), 3 )]
	public class JumpCommand : ComponentCommand
	{
		protected override void OnReset()
		{
			ComponentId = 0;
		}
	}

	[ProtoContract]
	[ProtoSubType( typeof( ComponentSnapshot ), 32 )]
	public class JumperSnapshot : ComponentSnapshot
	{
		[ProtoMember( 21 )]
		public uint JumpStopTime;

		[ProtoMember( 22 )]
		public uint JumpContext;

		public override ComponentSnapshot Clone()
		{
			var s = this.Allocate<JumperSnapshot>();
			s.ComponentId = ComponentId;
			s.JumpStopTime = JumpStopTime;
			s.JumpContext = JumpContext;
			return s;
		}

		protected override bool IsApproximate(ComponentSnapshot other)
		{
			if( !( other is JumperSnapshot s ) )
				return false;

			return
				IsApproximate( ComponentId, s.ComponentId ) &&
				IsApproximate( JumpStopTime, s.JumpStopTime ) &&
				IsApproximate( JumpContext, s.JumpContext );
		}

		protected override void OnReset()
		{
			ComponentId = 0;
			JumpStopTime = 0;
			JumpContext = 0;
		}
	}

	[ProtoContract]
	[ProtoSubType( typeof( ComponentEvent ), 5 )]
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
	[ProtoSubType( typeof( ComponentEvent ), 6 )]
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

		public EventHandler<Jumper, uint> OnJumpStarted;
		public EventHandler<Jumper, uint> OnJumpStopped;

		protected override void OnInitialize()
		{
			OnJumpStarted = CreateEventHandler<Jumper, uint>();
			OnJumpStopped = CreateEventHandler<Jumper, uint>();
		}

		protected override ComponentSnapshot CreateSnapshot()
		{
			return CreateSnapshot<JumperSnapshot>();
		}

		protected override void OnCommandReceived(ComponentCommand command)
		{
			switch( command )
			{
				case JumpCommand c:
					if( ( Entity.SceneManager.Simulator.IsServer || Entity.IsLocalEntity ) && !IsJumping )
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
			s.JumpContext++;
			OnJumpStarted.Invoke( this, s.JumpContext );
			return s;
		}

		JumperSnapshot OnJumpStoppedEvent(JumpStoppedEvent e)
		{
			var s = (JumperSnapshot)State.Clone();
			s.JumpStopTime = 0;
			OnJumpStopped.Invoke( this, s.JumpContext );
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