using System;

namespace EcsSync2
{
	[AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
	public class ProtoSubTypeAttribute : Attribute
	{
		public ProtoSubTypeAttribute(Type baseType, int tag)
		{
			BaseType = baseType;
			Tag = tag;
		}

		public Type BaseType { get; }
		public int Tag { get; }
	}
}
