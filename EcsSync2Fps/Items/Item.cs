using ProtoBuf;
using System;

namespace EcsSync2.Fps
{
	[ProtoContract]
	[ProtoSubType( typeof( EntitySettings ), 3 )]
	public class ItemSettings : EntitySettings
	{
		[ProtoMember( 1 )]
		public string Type = "SPPotion";
	}

	public class Item : Entity
	{
		public SPPotion SPPotion { get; private set; }

		protected override void OnInitialize()
		{
			var s = (ItemSettings)Settings;
			switch( s.Type )
			{
				case "SPPotion":
					SPPotion = AddComponent<SPPotion>();
					break;

				default:
					throw new NotSupportedException( s.Type );
			}
		}
	}
}
