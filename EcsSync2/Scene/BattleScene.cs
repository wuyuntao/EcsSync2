using System;

namespace EcsSync2
{
	public class BattleScene : Scene
	{
		internal override void OnInitialize(SceneManager sceneManager)
		{
			base.OnInitialize( sceneManager );

			// TODO Load colliders

			if( sceneManager.Simulator.IsServer )
			{
				GameManager = SceneManager.CreateEntity<GameManager>(
					sceneManager.Simulator.InstanceIdAllocator.Allocate(),
					new GameManagerSettings() );
			}
		}

		protected internal override Entity CreateEntity(InstanceId instanceId, EntitySettings settings)
		{
			throw new NotImplementedException();
		}

		protected override Snapshot OnEventApplied(ITickContext ctx, Snapshot state, Event @event)
		{
			throw new NotImplementedException();
		}

		protected override void OnSnapshotRecovered(ITickContext ctx, Snapshot cs)
		{
			throw new NotImplementedException();
		}

		public GameManager GameManager { get; private set; }
	}
}
