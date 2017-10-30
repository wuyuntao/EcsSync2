using System;

namespace EcsSync2.FpsExample
{
	public class SceneElementSettings : EntitySettings
	{
		public string Type = "Door";

		protected override EntitySettings Clone()
		{
			throw new NotImplementedException();
		}
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
