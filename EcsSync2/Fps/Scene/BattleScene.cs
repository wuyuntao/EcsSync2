using System;

namespace EcsSync2.Fps
{
	public class BattleScene : Scene
	{
		protected override void OnInitialize()
		{
		}

		protected internal override Entity CreateEntity(InstanceId id, EntitySettings settings)
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
	}
}
