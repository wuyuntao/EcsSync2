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

		protected internal override Entity CreateEntity(InstanceId id, IEntitySettings settings)
		{
			switch( settings )
			{
				case GameManagerSettings s:
					return CreateEntity<GameManager, GameManagerSettings>( id, s );

				case PlayerSettings s:
					return CreateEntity<Player, PlayerSettings>( id, s );

				case CharacterSettings s:
					return CreateEntity<Character, CharacterSettings>( id, s );

				case SceneElementSettings s:
					return CreateEntity<SceneElement, SceneElementSettings>( id, s );

				case ItemSettings s:
					return CreateEntity<Item, ItemSettings>( id, s );

				default:
					throw new NotSupportedException( settings.ToString() );
			}
		}

		public GameManager GameManager { get; private set; }
		public Player LocalPlayer { get; private set; }
		public Character LocalCharacter { get; private set; }
	}
}
