using System;

namespace EcsSync2
{
	public struct InstanceId : IEquatable<InstanceId>
	{
		const int OwnerIndexOffset = 27;

		public readonly uint Value;

		internal InstanceId(uint rawValue)
		{
			Value = rawValue;
		}

		internal InstanceId(byte ownerIndex, uint value)
		{
			Value = ( (uint)ownerIndex << OwnerIndexOffset ) | value;
		}

		public bool Equals(InstanceId other)
		{
			return Value.Equals( other.Value );
		}

		public override bool Equals(object obj)
		{
			if( obj is InstanceId )
				return Equals( (InstanceId)obj );
			else
				return false;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public static bool operator ==(InstanceId x, InstanceId y)
		{
			return x.Value == y.Value;
		}

		public static bool operator !=(InstanceId x, InstanceId y)
		{
			return x.Value != y.Value;
		}

		public static implicit operator uint(InstanceId id)
		{
			return id.Value;
		}

		public static implicit operator InstanceId(uint value)
		{
			return new InstanceId( value );
		}

		public static InstanceId Empty
		{
			get { return new InstanceId( 0 ); }
		}
	}

	public class InstanceIdAllocator
	{
		byte m_ownerIndex;
		uint m_counter;

		public InstanceIdAllocator(byte ownerIndex, uint counter = 0)
		{
			m_ownerIndex = ownerIndex;
			m_counter = counter;
		}

		public InstanceId Allocate()
		{
			return new InstanceId( m_ownerIndex, ++m_counter );
		}
	}
}
