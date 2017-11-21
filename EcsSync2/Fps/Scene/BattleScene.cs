using System;

namespace EcsSync2.Fps
{
	public class BattleScene : Scene
	{
		protected override void OnInitialize()
		{
			OnEntityCreated += Scene_OnEntityCreated;
		}

		void Scene_OnEntityCreated(Entity entity)
		{
			switch( entity )
			{
				case GameManager e:
					GameManager = e;
					break;

				case Player e:
					if( e.TheSettings.UserId == SceneManager.Simulator.LocalUserId )
						LocalPlayer = e;
					break;

				case Character e:
					if( e.TheSettings.UserId == SceneManager.Simulator.LocalUserId )
						LocalCharacter = e;
					break;
			}
		}

		protected internal override void CreateEntity(InstanceId id, IEntitySettings settings)
		{
			switch( settings )
			{
				case GameManagerSettings s:
					CreateEntity<GameManager, GameManagerSettings>( id, s );
					break;

				case PlayerSettings s:
					CreateEntity<Player, PlayerSettings>( id, s );
					break;

				case CharacterSettings s:
					CreateEntity<Character, CharacterSettings>( id, s );
					break;

				case SceneElementSettings s:
					CreateEntity<SceneElement, SceneElementSettings>( id, s );
					break;

				case ItemSettings s:
					CreateEntity<Item, ItemSettings>( id, s );
					break;

				default:
					throw new NotSupportedException( settings.ToString() );
			}
		}

		public GameManager GameManager { get; private set; }
		public Player LocalPlayer { get; private set; }
		public Character LocalCharacter { get; private set; }
	}
}
