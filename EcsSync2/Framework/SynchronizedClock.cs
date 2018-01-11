using System;
using System.Collections.Generic;
using System.Linq;

namespace EcsSync2
{
	public class SynchronizedClock : SimulatorComponent
	{
		float m_localTime;
		float m_remoteTime;
		Queue<float> m_rtts = new Queue<float>();
		float m_averageRtt;
		float m_time;
		float m_deltaTime;

		bool m_speedUp;
		float m_speedUpDeltaTime;

		public SynchronizedClock(Simulator simulator)
			: base( simulator )
		{
		}

		public void Synchronize(float serverTime, float rtt)
		{
			//Simulator.Context.Log( "Synchronize st: {0}, rtt: {1}, time: {2}", serverTime, rtt, m_time );

			m_rtts.Enqueue( rtt );
			if( m_rtts.Count > Configuration.AverageRttCount )
				m_rtts.Dequeue();
			m_averageRtt = m_rtts.Average();
			//m_averageRtt = rtt;

			m_remoteTime = ( serverTime + rtt / 2f + m_speedUpDeltaTime );

			if( Math.Abs( m_time - m_remoteTime ) > rtt * 2 )
			{
				Simulator.Context.LogWarning( "Clock desynchronizing happens. remoteTime: {0}, time: {1}, rtt: {2}", m_remoteTime, m_time, rtt );
				m_time = Math.Max( m_time, m_remoteTime );
			}
		}

		public void Tick(float deltaTime)
		{
			const float changeRatio = 0.05f;

			//if( m_speedUp )
			//{
			//	var deltaTimeChange = deltaTime * changeRatio;
			//	m_speedUpDeltaTime += deltaTimeChange;
			//	deltaTime += deltaTimeChange;
			//}
			//else if( m_speedUpDeltaTime > 0 )
			//{
			//	var deltaTimeChange = Math.Min( deltaTime * changeRatio, m_speedUpDeltaTime );
			//	m_speedUpDeltaTime -= deltaTimeChange;
			//	deltaTime -= deltaTimeChange;

			//	if( m_speedUpDeltaTime <= 1e-6 )
			//	{
			//		//Simulator.Context.Log( "m_speedUpDeltaTime return 0 {0}", m_time );
			//		m_speedUpDeltaTime = 0;
			//	}
			//}

			m_deltaTime = deltaTime;
			m_localTime += m_deltaTime;
			m_remoteTime += m_deltaTime;

			// 微调 deltaTime
			//var adjustmentThreshold = m_deltaTime * Configuration.SynchronizedClockAdjustmentRatio * 2;
			//if( m_time + m_deltaTime > m_remoteTime + adjustmentThreshold )
			//	m_deltaTime *= ( 1 - Configuration.SynchronizedClockAdjustmentRatio );
			//else if( m_time + m_deltaTime < m_remoteTime - adjustmentThreshold )
			//	m_deltaTime *= ( 1 + Configuration.SynchronizedClockAdjustmentRatio );

			m_time += m_deltaTime;

			//Simulator.Context.LogWarning( "{5} / {6} Tick deltaTime: {0}, localTime: {1}, remoteTime: {2}, time: {3}, rtt: {4}",
			//	m_deltaTime, m_localTime, m_remoteTime, m_time, m_rtt, Simulator.IsServer, Simulator.IsClient );
		}

		public float Time => m_time;

		public float DeltaTime => m_deltaTime;

		public float LocalTime => m_localTime;

		public float Rtt => m_averageRtt;

		public bool SpeedUp
		{
			get => m_speedUp;
			set
			{
				//if( m_speedUp != value )
				//	Simulator.Context.Log( "SpeedUp Changed {0}, {1}", m_time, value );

				m_speedUp = value;
			}
		}

		public int CommandDeltaTime { get; internal set; }
	}
}
