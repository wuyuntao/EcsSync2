using MessagePack;
using System;
using System.Runtime.InteropServices;

namespace EcsSync2
{
	[StructLayout( LayoutKind.Sequential )]
	[MessagePackObject]
	public struct Vector2D : IEquatable<Vector2D>, IFormattable
	{
		public static readonly Vector2D Zero = new Vector2D();

		public static readonly Vector2D One = new Vector2D( 1 );

		[Key( 0 )]
		public float X;
		[Key( 1 )]
		public float Y;

		[IgnoreMember]
		public float this[int i]
		{
			get
			{
				if( i == 0 )
					return X;
				else if( i == 1 )
					return Y;
				else
					throw new InvalidOperationException( $"Invalid index {i}" );
			}
			set
			{
				if( i == 0 )
					X = value;
				else if( i == 1 )
					Y = value;
				else
					throw new InvalidOperationException( $"Invalid index {i}" );
			}
		}

		public Vector2D(float xy)
		{
			X = xy;
			Y = xy;
		}

		public Vector2D(float x, float y)
		{
			X = x;
			Y = y;
		}

		public float Length()
		{
			return (float)Math.Sqrt( X * X + Y * Y );
		}

		public float LengthSquared()
		{
			return X * X + Y * Y;
		}

		public float Normalize()
		{
			var length = Length();
			if( length < float.Epsilon )
				return 0.0f;

			X /= length;
			Y /= length;

			return length;
		}

		public bool IsValid()
		{
			return float.IsInfinity( X ) && float.IsInfinity( Y );
		}

		public static Vector2D operator -(Vector2D v1)
		{
			return new Vector2D( -v1.X, -v1.Y );
		}

		public static Vector2D operator +(Vector2D v1, Vector2D v2)
		{
			return new Vector2D( v1.X + v2.X, v1.Y + v2.Y );
		}

		public static Vector2D operator -(Vector2D v1, Vector2D v2)
		{
			return new Vector2D( v1.X - v2.X, v1.Y - v2.Y );
		}

		public static Vector2D operator *(Vector2D v1, float a)
		{
			return new Vector2D( v1.X * a, v1.Y * a );
		}

		public static Vector2D operator *(float a, Vector2D v1)
		{
			return new Vector2D( v1.X * a, v1.Y * a );
		}

		public static Vector2D operator /(Vector2D v1, float a)
		{
			return new Vector2D( v1.X / a, v1.Y / a );
		}

		public static Vector2D operator /(float a, Vector2D v1)
		{
			return new Vector2D( v1.X / a, v1.Y / a );
		}

		public static bool operator ==(Vector2D a, Vector2D b)
		{
			return a.X == b.X && a.Y == b.Y;
		}

		public static bool operator !=(Vector2D a, Vector2D b)
		{
			return a.X != b.X || a.Y != b.Y;
		}

		public static float Dot(Vector2D a, Vector2D b)
		{
			// 向量点积可表示为 |a| * |b| * cos(theta)。也可理解为相当于将 a 投影到 b 上，并乘以 b 的长度。
			return a.X * b.X + a.Y * b.Y;
		}

		public static float Cross(Vector2D a, Vector2D b)
		{
			return a.X * b.Y - a.Y * b.X;
		}

		public static Vector2D Cross(Vector2D a, float s)
		{
			return new Vector2D( s * a.Y, -s * a.X );
		}

		public static Vector2D Cross(float s, Vector2D a)
		{
			return new Vector2D( -s * a.Y, s * a.X );
		}

		public static Vector2D Min(Vector2D a, Vector2D b)
		{
			return new Vector2D( Math.Min( a.X, b.X ), Math.Min( a.Y, b.Y ) );
		}

		public static Vector2D Max(Vector2D a, Vector2D b)
		{
			return new Vector2D( Math.Max( a.X, b.X ), Math.Max( a.Y, b.Y ) );
		}

		public static float Distance(Vector2D a, Vector2D b)
		{
			Vector2D c = a - b;
			return c.Length();
		}

		public static float DistanceSquared(Vector2D a, Vector2D b)
		{
			Vector2D c = a - b;
			return Dot( c, c );
		}

		#region IEquatable

		public override bool Equals(object obj)
		{
			if( !( obj is Vector2D ) )
				return false;

			return Equals( (Vector2D)obj );
		}

		public bool Equals(Vector2D obj)
		{
			return obj.X == X && obj.Y == Y;
		}

		public override int GetHashCode()
		{
			return ( X.GetHashCode() * 0x18d ^ Y.GetHashCode() ).GetHashCode();
		}

		#endregion

		#region IFormattable

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return string.Format( "({0}, {1})", X.ToString( format, formatProvider ), Y.ToString( format, formatProvider ) );
		}

		public string ToString(string format)
		{
			return string.Format( "({0}, {1})", X.ToString( format ), Y.ToString( format ) );
		}

		public string ToString(IFormatProvider formatProvider)
		{
			return string.Format( "({0}, {1})", X.ToString( formatProvider ), Y.ToString( formatProvider ) );
		}

		public override string ToString()
		{
			return string.Format( "({0}, {1})", X.ToString(), Y.ToString() );
		}

		#endregion
	}
}
