using System.Collections.Generic;

namespace EcsSync2
{
    public class EventDispatcher : SimulatorComponent
    {
        SortedList<uint, DeltaSyncFrame> m_deltaSyncFrames = new SortedList<uint, DeltaSyncFrame>();

        List<EventHandler> m_dirtyHandlers = new List<EventHandler>();
        List<EventInvocation> m_invocations = new List<EventInvocation>();

        public EventDispatcher(Simulator simulator)
            : base(simulator)
        {
        }

        #region EventHandler

        internal void AddDirtyEventHandler(EventHandler handler)
        {
            m_dirtyHandlers.Add(handler);
        }

        internal void AddEventInvocation(EventInvocation invocation)
        {
            invocation.Retain();
            m_invocations.Add(invocation);
        }

        internal void Invoke()
        {
            for (int i = 0; i < m_invocations.Count; i++)
                m_invocations[i].Invoke();
            m_invocations.Clear();

            for (int i = 0; i < m_dirtyHandlers.Count; i++)
                m_dirtyHandlers[i].ApplyChanges();
            m_dirtyHandlers.Clear();
        }

        #endregion

        #region EventFrame

        internal void AddEventToFrame(uint time, Event @event)
        {
            //Simulator.Context.Log( "EnqueueEvent {0}ms {1}", time, @event );

            var frame = EnsureFrame(time);
            @event.Retain();
            frame.Events.Add(@event);
        }

        DeltaSyncFrame EnsureFrame(uint time)
        {
            if (!m_deltaSyncFrames.TryGetValue(time, out DeltaSyncFrame frame))
            {
                frame = Simulator.ReferencableAllocator.Allocate<DeltaSyncFrame>();
                frame.Time = time;

                frame.Retain();
                m_deltaSyncFrames.Add(time, frame);
            }
            return frame;
        }

        internal DeltaSyncFrame FetchEvents(uint time)
        {
            var f = EnsureFrame(time);
            m_deltaSyncFrames.Remove(time);
            return f;
        }

        #endregion

        #region EventArgs

        internal abstract class EventArgs : Referencable
        {
        }

        #endregion

        #region EventListener

        internal abstract class EventListener : Referencable
        {
            public abstract void Invoke(EventArgs args);
        }

        #endregion

        #region EventInvocation

        internal class EventInvocation : Referencable
        {
            public EventListener Listener;

            public EventArgs Args;

            public void Invoke()
            {
                Listener.Invoke(Args);
            }

            protected override void OnReset()
            {
                base.OnReset();

                Listener.Release();
                Listener = null;

                Args.Release();
                Args = null;
            }
        }

        #endregion

        #region EventHandler

        internal class EventHandler
        {
            EventDispatcher m_dispatcher;
            List<EventListener> m_listeners = new List<EventListener>();
            bool m_isDirty;

            public EventHandler(EventDispatcher dispatcher)
            {
                m_dispatcher = dispatcher;
            }

            public void AddListener(EventListener listener)
            {
                listener.Retain();
                m_listeners.Add(listener);

                IsDirty = true;
            }

            public void RemoveListener(int index)
            {
                m_listeners[index].Release();
                m_listeners[index] = null;

                IsDirty = true;
            }

            public void ApplyChanges()
            {
                m_listeners.RemoveAll(l => l == null);

                IsDirty = false;
            }

            public void Invoke(EventArgs args)
            {
                for (int i = 0; i < m_listeners.Count; i++)
                {
                    var listener = m_listeners[i];
                    if (listener != null)
                    {
                        var invocation = m_dispatcher.Simulator.ReferencableAllocator.Allocate<EventInvocation>();
                        invocation.Listener = listener;
                        invocation.Args = args;

                        m_dispatcher.AddEventInvocation(invocation);
                    }
                }
            }

            public EventDispatcher Dispatcher => m_dispatcher;

            public IList<EventListener> Listeners => m_listeners;

            bool IsDirty
            {
                set
                {
                    if (value && !m_isDirty)
                        m_dispatcher.AddDirtyEventHandler(this);

                    m_isDirty = value;
                }
            }
        }

        #endregion
    }
}
