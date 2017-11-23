namespace EcsSync2
{
	public static class Configuration
	{
		public const int StopExhaustCommandBufferSize = 6;

		public const uint SimulationDeltaTime = 16;

		public const uint SynchronizationDeltaTime = SimulationDeltaTime * 3;

		public const float SynchorizedClockDesyncThreshold = 1f;

		public const float SynchorizedClockAdjustmentThreshold = 1f;

		public const float SynchronizedClockAdjustmentRatio = 0.1f;

		public const int TimelineDefaultCapacity = 500 / 16;

		public const float ComponentReconcilationRatio = 0.1f;

		public const int HeartbeatIntervalTime = 1000;
	}
}
