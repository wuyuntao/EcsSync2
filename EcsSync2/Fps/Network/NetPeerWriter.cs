using LiteNetLib;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;

namespace EcsSync2.Fps
{
	class NetPeerWriter
	{
		public ILogger Logger { get; }

		byte[] m_buffer;
		MemoryStream m_stream;

		int m_writeCount;
		int m_maxBufferSize;

		public NetPeerWriter(ILogger logger, int capacity = 1024 * 16)
		{
			Logger = logger;

			m_buffer = new byte[capacity];
			m_stream = new MemoryStream( m_buffer );
		}

		public void Write(NetPeer netPeer, Message message)
		{
			SerializeMessage( message );

			netPeer.Send( m_buffer, 0, (int)m_stream.Length, SendOptions.ReliableOrdered );

			IncreaseWriteCont();
		}

		public void Write(IEnumerable<NetPeer> netPeers, Message message)
		{
			SerializeMessage( message );

			foreach( var netPeer in netPeers )
				netPeer.Send( m_buffer, 0, (int)m_stream.Length, SendOptions.ReliableOrdered );

			IncreaseWriteCont();
		}

		void SerializeMessage(Message message)
		{
			var env = new MessageEnvelop() { Message = message };
			m_stream.SetLength( 0 );
			Serializer.Serialize( m_stream, env );
		}

		void IncreaseWriteCont()
		{
			m_writeCount++;
			m_maxBufferSize = Math.Max( m_maxBufferSize, (int)m_stream.Length );

			if( ( m_writeCount % 1000 ) == 0 )
				Logger?.Log( "WriteCount: {0}, MaxWriteBufferSize: {1}", m_writeCount, m_maxBufferSize );
		}
	}
}
