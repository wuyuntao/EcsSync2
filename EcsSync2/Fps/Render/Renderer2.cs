namespace EcsSync2.Fps
{
    public abstract class Renderer2 : Component
    {
        RenderManager RenderManager { get; set; }

        #region Life-cycle

        internal void Update()
        {
            EnsureTickContext();

            OnUpdate();
        }

        #endregion

        #region Public Interface

        protected override void OnInitialize()
        {
            RenderManager = Entity.SceneManager.Simulator.RenderManager;
            RenderManager.AddRenderer(this);
        }

        protected abstract void OnUpdate();

        #endregion
    }
}
