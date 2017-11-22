using MessagePack;
using System;
using System.Diagnostics;

namespace EcsSync2.Fps
{
    public class TransformSettings : ComponentSettings
    {
        public Vector2D Position;

        public Vector2D Velocity;

        public bool IsStatic;
    }

    [MessagePackObject]
    public class TransformSnapshot : ComponentSnapshot, IComponentSnapshot
    {
        [Key(20)]
        public Vector2D Position;

        [Key(21)]
        public Vector2D Velocity;
    }

    [MessagePackObject]
    public class TransformMovedEvent : ComponentEvent
    {
        [Key(20)]
        public Vector2D Position;
    }

    [MessagePackObject]
    public class TransformVelocityChangedEvent : ComponentEvent
    {
        [Key(20)]
        public Vector2D Velocity;
    }

    public class Transform : Component
    {
        public Action OnMoved;

        protected override void OnCommandReceived(ComponentCommand command)
        {
            throw new NotSupportedException(command.ToString());
        }

        protected override ComponentSnapshot OnEventApplied(ComponentEvent @event)
        {
            switch (@event)
            {
                case TransformMovedEvent e:
                    var s1 = (TransformSnapshot)State.Clone();
                    //Entity.SceneManager.Simulator.Context.Log($"received {nameof(TransformMovedEvent)} {e.Position} <- {s1.Position}");
                    s1.Position = e.Position;
                    OnMoved?.Invoke();
                    return s1;

                case TransformVelocityChangedEvent e:
                    var s2 = (TransformSnapshot)State.Clone();
                    //Entity.SceneManager.Simulator.Context.Log($"received {nameof(TransformVelocityChangedEvent)} {e.Velocity} <- {s2.Velocity}");
                    s2.Velocity = e.Velocity;
                    return s2;

                default:
                    throw new NotSupportedException(@event.ToString());
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

        internal void ApplyTransformMovedEvent(Vector2D offset)
        {
            Debug.Assert(offset.IsValid());

            var s = (TransformSnapshot)State;
            var e = CreateEvent<TransformMovedEvent>();
            e.Position = s.Position + offset;
            ApplyEvent(e);
        }

        internal void ApplyTransformVelocityChangedEvent(Vector2D velocity)
        {
            Debug.Assert(velocity.IsValid());

            var s = (TransformSnapshot)State;
            if (s.Velocity == velocity)
                return;

            var e = CreateEvent<TransformVelocityChangedEvent>();
            e.Velocity = velocity;
            ApplyEvent(e);
        }

        protected override void OnInitialize()
        {
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
