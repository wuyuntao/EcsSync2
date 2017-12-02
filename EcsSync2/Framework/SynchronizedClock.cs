using System;

namespace EcsSync2
{
	public class SynchronizedClock : SimulatorComponent
	{
		float m_localTime;
		float m_remoteTime;
		float m_rtt;
		float m_time;
		float m_deltaTime;

		public SynchronizedClock(Simulator simulator)
			: base( simulator )
		{
		}

		public void Synchronize(float serverTime, float rtt)
		{
			//Simulator.Context.Log( "Synchronize st: {0}, rtt: {1}, time: {2}", serverTime, rtt, m_time );

			m_remoteTime = ( serverTime + rtt / 2f );
			m_rtt = rtt;

			if( Math.Abs( m_time - m_remoteTime ) > Configuration.SynchorizedClockDesyncThreshold )
				m_time = Math.Max( m_time, m_remoteTime );
		}

		public void Tick(float deltaTime)
		{
			m_deltaTime = deltaTime;
			m_localTime += m_deltaTime;
			m_remoteTime += m_deltaTime;

			if( m_time + m_deltaTime > m_remoteTime + Configuration.SynchorizedClockAdjustmentThreshold )
				m_deltaTime *= ( 1 - Configuration.SynchronizedClockAdjustmentRatio );
			else if( m_time + m_deltaTime < m_remoteTime - Configuration.SynchorizedClockAdjustmentThreshold )
				m_deltaTime *= ( 1 + Configuration.SynchronizedClockAdjustmentRatio );

			m_time += m_deltaTime;

			//Simulator.Context.LogWarning( "{5} / {6} Tick deltaTime: {0}, localTime: {1}, remoteTime: {2}, time: {3}, rtt: {4}",
			//	m_deltaTime, m_localTime, m_remoteTime, m_time, m_rtt, Simulator.IsServer, Simulator.IsClient );
		}

		public float Time => m_time;

		public float DeltaTime => m_deltaTime;

		public float LocalTime => m_localTime;

		public float Rtt => m_rtt;
	}
}
