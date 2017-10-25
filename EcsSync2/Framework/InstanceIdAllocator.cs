using System;

namespace EcsSync2
{
	public struct InstanceId : IEquatable<InstanceId>
	{
		const int EntityIdOffset = 5;

		public readonly uint Value;

		InstanceId(uint rawValue)
		{
			Value = rawValue;
		}

		public static InstanceId CreateEntityId(uint value)
		{
			return new InstanceId( value << EntityIdOffset );
		}

		public uint CreateEntityId()
		{
			return new InstanceId( Value >> EntityIdOffset << EntityIdOffset );
		}

		public InstanceId CreateComponentId(uint componentId)
		{
			return new InstanceId( Value | componentId );
		}

		public bool IsEntityId()
		{
			return CreateEntityId( this ) == this;
		}

		public bool IsComponentId()
		{
			return CreateEntityId( this ) != this;
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

	public class InstanceIdAllocator : SimulatorComponent
	{
		uint m_counter;

		public InstanceIdAllocator(Simulator simulator)
			: base( simulator )
		{
		}

		public InstanceId Allocate()
		{
			return InstanceId.CreateEntityId( ++m_counter );
		}
	}
}
