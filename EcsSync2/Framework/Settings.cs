namespace EcsSync2
{
	public static class Settings
	{
		public const int DefaultCommandBufferSize = 3;

		public const int StopExhaustCommandBufferSize = 6;

		public const uint SimulationDeltaTime = 16;

		public const int SynchronizationDeltaTime = 16 * 3;

		public const float SynchorizedClockDesyncThreshold = 1f;

		public const float SynchorizedClockAdjustmentThreshold = 1f;

		public const float SynchronizedClockAdjustmentRatio = 0.1f;

		public const int TimelineDefaultCapacity = 500 / 16;
	}
}
