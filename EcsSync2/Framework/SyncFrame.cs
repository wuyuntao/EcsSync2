﻿using EcsSync2.Fps;
using ProtoBuf;
using System.Collections.Generic;

namespace EcsSync2
{
	[ProtoContract]
	[ProtoInclude( 1, typeof( FullSyncFrame ) )]
	[ProtoInclude( 2, typeof( DeltaSyncFrame ) )]
	public abstract class SyncFrame : Message
	{
		[ProtoMember( 11 )]
		public uint Time;
	}

	[ProtoContract]
	public class FullSyncFrame : SyncFrame
	{
		[ProtoMember( 21 )]
		public List<EntitySnapshot> Entities = new List<EntitySnapshot>();

		public override string ToString()
		{
			return $"{GetType().Name}<Time: {Time}, Entities: {Entities.Count}>";
		}

		protected override void OnAllocate()
		{
			base.OnAllocate();

			foreach( var e in Entities )
				this.Allocate( e );
		}

		protected override void OnReset()
		{
			foreach( var e in Entities )
				e.Release();

			Entities.Clear();

			base.OnReset();
		}
	}

	[ProtoContract]
	public class DeltaSyncFrame : SyncFrame
	{
		[ProtoMember( 21 )]
		public List<Event> Events = new List<Event>();

		public override string ToString()
		{
			return $"{GetType().Name}<Time: {Time}, Events: {Events.Count}>";
		}

		protected override void OnAllocate()
		{
			base.OnAllocate();

			foreach( var e in Events )
				this.Allocate( e );
		}

		protected override void OnReset()
		{
			foreach( Event e in Events )
				e.Release();

			Events.Clear();

			base.OnReset();
		}
	}
}
