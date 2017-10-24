namespace EcsSync2
{
	public static class Settings
	{
		public static int DefaultCommandBufferSize = 3;

		public static int StopExhaustCommandBufferSize = 6;

		public static uint FixedDeltaTime = 16;

		public static int SyncFixedDeltaTime = 16 * 3;

		public static float SynchorizedClockDesyncThreshold = 1f;

		public static float SynchorizedClockAdjustmentThreshold = 1f;

		public static float SynchronizedClockAdjustmentRatio = 0.1f;
	}
}
