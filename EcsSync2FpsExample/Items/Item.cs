using System;

namespace EcsSync2.FpsExample
{
	public class ItemSettings : EntitySettings
	{
		public string Type = "SPPotion";

		protected override EntitySettings Clone()
		{
			throw new NotImplementedException();
		}
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
