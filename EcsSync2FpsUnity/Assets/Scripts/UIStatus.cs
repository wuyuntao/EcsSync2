using UnityEngine;
using UnityEngine.UI;

namespace EcsSync2
{
	public class UIStatus : MonoBehaviour
	{
		public Text Text;
		public float UpdateInterval = 0.2f;

		public float FPS;
		public float MinUnityDeltaTime = 1;
		public float MaxUnityDeltaTime = 0;
		public float MinSimulatorDeltaTime = 1;
		public float MaxSimulatorDeltaTime = 0;
		public float RTT;
		public float IND;
		public float CAT;

		void Start()
		{
			InvokeRepeating( "UpdateStatus", UpdateInterval, UpdateInterval );
		}

		void Update()
		{
			MinUnityDeltaTime = Mathf.Min( MinUnityDeltaTime, Time.deltaTime );
			MaxUnityDeltaTime = Mathf.Max( MaxUnityDeltaTime, Time.deltaTime );
		}

		void UpdateStatus()
		{
			FPS = ( 1 / Time.smoothDeltaTime );

			Text.text = string.Format( "FPS: {0:f1} UDT: {1:f1}-{2:f1} SDT: {3:f1}-{4:f1} RTT: {5:f1} IND: {6:f1} CAT {7:f1}",
				FPS,
				MinUnityDeltaTime * 1000, MaxUnityDeltaTime * 1000,
				MinSimulatorDeltaTime * 1000, MaxSimulatorDeltaTime * 1000,
				RTT * 1000, IND * 1000, CAT * 1000 );

			MinUnityDeltaTime = 1;
			MaxUnityDeltaTime = 0;
			MinSimulatorDeltaTime = 1;
			MaxSimulatorDeltaTime = 0;
		}
	}
}
