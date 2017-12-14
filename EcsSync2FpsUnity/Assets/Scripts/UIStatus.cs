using UnityEngine;
using UnityEngine.UI;

namespace EcsSync2
{
	public class UIStatus : MonoBehaviour
	{
		public Text Text;
		public float UpdateInterval = 0.2f;

		public float FPS;
		public float MinDeltaTime = 1;
		public float MaxDeltaTime = 0;
		public float RTT;
		public float IND;

		void Start()
		{
			InvokeRepeating( nameof( UpdateStatus ), UpdateInterval, UpdateInterval );
		}

		void Update()
		{
			MinDeltaTime = Mathf.Min( MinDeltaTime, Time.deltaTime );
			MaxDeltaTime = Mathf.Max( MaxDeltaTime, Time.deltaTime );
		}

		void UpdateStatus()
		{
			FPS = ( 1 / Time.smoothDeltaTime );

			Text.text = string.Format( "FPS: {0:f1} DT: {1:f1}-{2:f1} RTT: {3:f1} IND: {4:f1}",
				FPS, MinDeltaTime * 1000, MaxDeltaTime * 1000, RTT * 1000, IND * 1000 );

			MinDeltaTime = 1;
			MaxDeltaTime = 0;
		}
	}
}
