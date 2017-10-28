using System;

namespace EcsSync2.FpsExample
{
    public class TransformSettings : ComponentSettings
    {
        public Vector2D Position;

        public Vector2D Velocity;

        public bool IsStatic;
    }

    public class TransformSnapshot : Snapshot
    {
        public Vector2D Position;

        public Vector2D Velocity;

        public override Snapshot Clone()
        {
            throw new NotImplementedException();
        }

        protected override Snapshot Extrapolate(uint time, uint extrapolateTime)
        {
            throw new NotImplementedException();
        }

        protected override Snapshot Interpolate(Snapshot other, float factor)
        {
            throw new NotImplementedException();
        }

        protected override Snapshot Interpolate(uint time, Snapshot targetSnapshot, uint targetTime, uint interpolateTime)
        {
            throw new NotImplementedException();
        }

        protected override bool IsApproximate(Snapshot other)
        {
            throw new NotImplementedException();
        }
    }

    public class TransformMovedEvent : Event
    {
        public Vector2D Position;
    }

    public class TransformVelocityChangedEvent : Event
    {
        public Vector2D Velocity;
    }

    public class Transform : Component
    {
        protected override void OnCommandReceived(Command command)
        {
            throw new NotImplementedException();
        }

        protected override Snapshot OnEventApplied(Event @event)
        {
            throw new NotImplementedException();
        }

        protected override void OnFixedUpdate()
        {
            throw new NotImplementedException();
        }

        protected override void OnSnapshotRecovered(Snapshot state)
        {
            throw new NotImplementedException();
        }

        protected override Snapshot OnStart()
        {
            throw new NotImplementedException();
        }

        internal void ApplyTransformMovedEvent(Vector2D offset)
        {
            var s = (TransformSnapshot)State;
            var e = s.Allocate<TransformMovedEvent>();
            e.Position = s.Position + offset;
            ApplyEvent(e);
        }

        internal void ApplyTransformVelocityChangedEvent(Vector2D velocity)
        {
            var s = (TransformSnapshot)State;
            if (s.Velocity == velocity)
                return;

            var e = State.Allocate<TransformVelocityChangedEvent>();
            e.Velocity = velocity;
            ApplyEvent(e);
        }
    }
}
