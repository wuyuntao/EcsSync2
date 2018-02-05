namespace EcsSync2
{
	public abstract class Renderer : Component
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
			if( Entity.SceneManager.Simulator.RenderManager != null )
			{
				RenderManager = Entity.SceneManager.Simulator.RenderManager;
				RenderManager.AddRenderer( this );
			}
		}

		protected abstract void OnUpdate();

		#endregion
	}
}
