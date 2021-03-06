﻿using ProtoBuf;
using System;

namespace EcsSync2
{
	[ProtoContract]
	public class CreateEntityCommand : SceneCommand
	{
		[ProtoMember( 11 )]
		public EntitySettings Settings;

		protected override void OnReset()
		{
			Settings = null;
		}
	}

	[ProtoContract]
	public class RemoveEntityCommand : SceneCommand
	{
		[ProtoMember( 11 )]
		public uint EntityId;

		protected override void OnReset()
		{
			EntityId = 0;
		}
	}

	[ProtoContract]
	public class EntityCreatedEvent : SceneEvent
	{
		[ProtoMember( 11 )]
		public uint EntityId;

		[ProtoMember( 12 )]
		public EntitySettings Settings;

		protected override void OnReset()
		{
			EntityId = 0;
			Settings = null;
		}
	}

	[ProtoContract]
	public class EntityRemovedEvent : SceneEvent
	{
		[ProtoMember( 11 )]
		public uint EntityId;

		protected override void OnReset()
		{
			EntityId = 0;
		}
	}

	public abstract class Scene : Disposable
	{
		public EventHandler<Scene, Entity> OnEntityCreated;
		public EventHandler<Scene, Entity> OnEntityRemoved;

		public SceneManager SceneManager { get; private set; }

		internal virtual void Initialize(SceneManager sceneManager)
		{
			SceneManager = sceneManager;

			OnEntityCreated = new EventHandler<Scene, Entity>( sceneManager.Simulator.EventDispatcher );
			OnEntityRemoved = new EventHandler<Scene, Entity>( sceneManager.Simulator.EventDispatcher );

			OnInitialize();
		}

		protected abstract void OnInitialize();

		protected override void DisposeManaged()
		{
			SafeDispose( ref OnEntityCreated );
			SafeDispose( ref OnEntityRemoved );

			base.DisposeManaged();
		}

		protected internal abstract void CreateEntity(InstanceId id, EntitySettings settings);

		protected TEntity CreateEntity<TEntity, TEntitySettings>(InstanceId id, EntitySettings settings)
			where TEntity : Entity, new()
			where TEntitySettings : EntitySettings
		{
			return SceneManager.CreateEntity<TEntity>( id, settings );
		}

		public void ApplyEntityCreatedEvent(EntitySettings settings)
		{
			var e = SceneManager.Simulator.ReferencableAllocator.Allocate<EntityCreatedEvent>();
			e.EntityId = SceneManager.Simulator.InstanceIdAllocator.Allocate();
			e.Settings = settings;
			ApplyEvent( e );
		}

		public void ApplyEntityRemovedEvent(InstanceId id)
		{
			var e = SceneManager.Simulator.ReferencableAllocator.Allocate<EntityRemovedEvent>();
			e.EntityId = SceneManager.Simulator.InstanceIdAllocator.Allocate();
			ApplyEvent( e );
		}

		internal void ReceiveCommand(SceneCommand command)
		{
			OnCommandReceived( command );
		}

		protected virtual void OnCommandReceived(SceneCommand command)
		{
			if( !SceneManager.Simulator.IsServer )
				return;

			switch( command )
			{
				case CreateEntityCommand c:
					ApplyEntityCreatedEvent( c.Settings );
					break;

				case RemoveEntityCommand c:
					ApplyEntityRemovedEvent( c.EntityId );
					break;

				default:
					throw new NotSupportedException( command.ToString() );
			}
		}

		internal void ApplyEvent(SceneEvent @event)
		{
			OnEventApplied( @event );

			if( SceneManager.Simulator.ServerTickScheduler != null )
				SceneManager.Simulator.ServerTickScheduler.EnqueueEvent( @event );

			@event.Release();
		}

		protected virtual void OnEventApplied(SceneEvent @event)
		{
			switch( @event )
			{
				case EntityCreatedEvent e:
					CreateEntity( e.EntityId, e.Settings );
					SceneManager.Simulator.Context.Log( "OnEntityCreatedEvent {0}, {1}, {2}", e.EntityId, e.Settings, SceneManager.Simulator.TickScheduler.CurrentContext );
					break;

				case EntityRemovedEvent e:
					SceneManager.RemoveEntity( e.EntityId );
					SceneManager.Simulator.Context.Log( "OnEntityRemovedEvent {0}, {1}", e.EntityId, SceneManager.Simulator.TickScheduler.CurrentContext );
					break;

				default:
					throw new NotSupportedException( @event.ToString() );
			}
		}
	}
}
