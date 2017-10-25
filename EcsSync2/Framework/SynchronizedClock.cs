using System;

namespace EcsSync2
{
	public class SynchronizedClock : SimulatorComponent
	{
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
			m_remoteTime = ( serverTime + rtt / 2f );
			m_rtt = rtt;

			if( Math.Abs( m_time - m_remoteTime ) > Settings.SynchorizedClockDesyncThreshold )
				m_time = Math.Max( m_time, m_remoteTime );
		}

		public void Tick(float deltaTime)
		{
			m_deltaTime = deltaTime;
			m_remoteTime += m_deltaTime;

			if( m_time + m_deltaTime > m_remoteTime + Settings.SynchorizedClockAdjustmentThreshold )
				m_deltaTime *= ( 1 - Settings.SynchronizedClockAdjustmentRatio );
			else if( m_time + m_deltaTime < m_remoteTime - Settings.SynchorizedClockAdjustmentThreshold )
				m_deltaTime *= ( 1 + Settings.SynchronizedClockAdjustmentRatio );

			m_time += m_deltaTime;
		}

		public float Time
		{
			get { return m_time; }
		}

		public float DeltaTime
		{
			get { return m_deltaTime; }
		}
	}
}
