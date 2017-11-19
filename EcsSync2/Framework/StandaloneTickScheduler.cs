namespace EcsSync2
{
    public class StandaloneTickScheduler : TickScheduler
    {
        TickContext m_context;

        public StandaloneTickScheduler(Simulator simulator)
            : base(simulator)
        {
        }

        internal override void Tick()
        {
            m_context = new TickContext(TickContextType.Sync, Simulator.FixedTime);

            EnterContext(m_context);

            Simulator.InputManager.SetInput();

            var f1 = Simulator.CommandQueue.FetchCommands(0, m_context.Time);
            if (f1 != null)
                DispatchCommands(f1);

            var f2 = Simulator.InputManager.EnqueueCommands();
            if (f2 != null)
                DispatchCommands(f2);

            FixedUpdate();

            Simulator.InputManager.ResetInput();

            LeaveContext();

            //Simulator.Context.Log( "Tick {0}", m_context.Time );
        }
    }
}
