using MessagePack;
using System;

namespace EcsSync2.Fps
{
	[MessagePackObject]
	public class SceneElementSettings : EntitySettings, IEntitySettingsUnion
	{
		[Key( 0 )]
		public string Type = "Door";
	}

	public class SceneElement : Entity
	{
		public Door Door { get; private set; }

		protected override void OnInitialize()
		{
			var s = (SceneElementSettings)Settings;
			switch( s.Type )
			{
				case "Door":
					Door = AddComponent<Door>();
					break;

				default:
					throw new NotSupportedException( s.Type );
			}
		}
	}
}
