using System;

namespace EcsSync2.FpsExample
{
	public class BattleScene : Scene
	{
		protected override void OnInitialize()
		{
			if( SceneManager.Simulator.IsServer )
				CreateEntity( SceneManager.Simulator.InstanceIdAllocator.Allocate(), new GameManagerSettings() );
		}

		protected override Entity CreateEntity(InstanceId id, EntitySettings settings)
		{
			switch( settings )
			{
				case GameManagerSettings s:
					return SceneManager.CreateEntity<GameManager>( id, s );

				case PlayerSettings s:
					return SceneManager.CreateEntity<Player>( id, s );

				default:
					throw new NotSupportedException( settings.ToString() );
			}
		}

		public GameManager GameManager { get; private set; }
	}
}
