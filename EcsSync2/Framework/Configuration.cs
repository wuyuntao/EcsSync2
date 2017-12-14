namespace EcsSync2
{
	public static class Configuration
	{
		public const int StopExhaustCommandBufferSize = 6;

		public const uint SimulationDeltaTime = 50;

		public const uint SynchronizationDeltaTime = SimulationDeltaTime * 3;

		public const float SynchorizedClockDesyncThreshold = 0.1f;

		public const float SynchorizedClockAdjustmentThreshold = 0.1f;

		public const float SynchronizedClockAdjustmentRatio = 0.1f;

		public const int TimelineDefaultCapacity = 500 / 16;

		public const float ComponentReconciliationRatio = 0.1f;

		public const int HeartbeatIntervalTime = 1000;

		public const int MaxCommandDispatchCount = 10;

		public const int MaxTickCount = 10;

		public const int AverageRttCount = 10;
	}
}
