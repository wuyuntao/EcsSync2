using MessagePack;
using MessagePack.Formatters;
using System;

namespace EcsSync2.FpsExample
{
	public class CustomResolver : IFormatterResolver
	{
		public static IFormatterResolver Instance = new CustomResolver();

		public IMessagePackFormatter<T> GetFormatter<T>()
		{
			return FormatterCache<T>.Formatter;
		}

		static class FormatterCache<T>
		{
			public static readonly IMessagePackFormatter<T> Formatter;

			static FormatterCache()
			{
				if( typeof( T ) == typeof( Vector2D ) )
					Formatter = (IMessagePackFormatter<T>)Vector2DFormatter.Instance;
				else
					throw new NotSupportedException();
			}
		}
	}

	public class Vector2DFormatter : IMessagePackFormatter<Vector2D>
	{
		public static IMessagePackFormatter<Vector2D> Instance = new Vector2DFormatter();

		public int Serialize(ref byte[] bytes, int offset, Vector2D value, IFormatterResolver formatterResolver)
		{
			var size = 0;
			size += MessagePackBinary.WriteSingle( ref bytes, offset, value.X );
			size += MessagePackBinary.WriteSingle( ref bytes, offset + size, value.Y );
			return size;
		}

		public Vector2D Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
		{
			var x = MessagePackBinary.ReadSingle( bytes, offset, out int readSizeX );
			var y = MessagePackBinary.ReadSingle( bytes, offset + readSizeX, out int readSizeY );
			readSize = readSizeX + readSizeY;
			return new Vector2D( x, y );
		}
	}
}
