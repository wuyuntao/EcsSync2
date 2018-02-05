using ProtoBuf;
using System;

namespace EcsSync2.Fps
{
	[ProtoContract]
	[ProtoSubType( typeof( EntitySettings ), 5 )]
	public class SceneElementSettings : EntitySettings
	{
		[ProtoMember( 1 )]
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
