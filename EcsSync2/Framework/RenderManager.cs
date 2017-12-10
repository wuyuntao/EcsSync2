using System;
using EcsSync2.Fps;
using System.Collections.Generic;

namespace EcsSync2
{
    public class RenderManager : SimulatorComponent
    {
        TickScheduler.TickContext m_context = new TickScheduler.TickContext(TickScheduler.TickContextType.Interpolation, 0);
        List<Renderer2> m_renderers = new List<Renderer2>();

        public RenderManager(Simulator simulator)
            : base(simulator)
        {
        }

        internal void AddRenderer(Renderer2 renderer)
        {
            m_renderers.Add(renderer);
        }

        internal void BeginRender()
        {
            var time = (uint)Math.Round(Math.Max(0f, Simulator.SynchronizedClock.Time * 1000f - Configuration.SimulationDeltaTime));
            if (time <= m_context.Time)
                return;

            m_context = new TickScheduler.TickContext(TickScheduler.TickContextType.Interpolation, time);

            Simulator.TickScheduler.EnterContext(m_context);

            m_renderers.RemoveAll(r =>
            {
                r.Update();
                return r.IsDestroyed;
            });
        }

        internal void EndRender()
        {
            Simulator.TickScheduler.LeaveContext();
        }

        internal TickScheduler.TickContext? CurrentContext => m_context;

        public uint InterpolationDelay { get; private set; } = 50;
    }
}
