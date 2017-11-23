using UnityEngine;
using UnityEngine.UI;

namespace EcsSync2
{
	public class UIStatus : MonoBehaviour
	{
		public Text Text;

		public float FPS;
		public float RTT;

		void Start()
		{
			InvokeRepeating( nameof( UpdateStatus ), 0.1f, 0.1f );
		}

		void UpdateStatus()
		{
			FPS = ( 1 / Time.smoothDeltaTime );

			Text.text = string.Format( "FPS: {0:f1} RTT: {1:f1}", FPS, RTT * 1000 );
		}
	}
}
