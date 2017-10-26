using System;

namespace EcsSync2
{
	public class BattleScene : Scene
	{
		protected override void OnInitialize()
		{
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
