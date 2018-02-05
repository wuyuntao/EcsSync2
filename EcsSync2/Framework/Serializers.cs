using ProtoBuf.Meta;
using System;
using System.IO;

namespace EcsSync2
{
	public static class Serializers
	{
		static Serializers()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach( var assembly in assemblies )
			{
				foreach( var type in assembly.GetTypes() )
				{
					var attrs = type.GetCustomAttributes( false );
					if( Array.Find( attrs, ( a => a is ProtoSubTypeAttribute ) ) is ProtoSubTypeAttribute subType )
						RuntimeTypeModel.Default[subType.BaseType].AddSubType( subType.Tag, type );
				}
			}
		}

		public static void Serialize<T>(Stream destination, T instance)
		{
			RuntimeTypeModel.Default.Serialize( destination, instance );
		}

		public static T Deserialize<T>(Stream source)
		{
			return (T)RuntimeTypeModel.Default.Deserialize( source, null, typeof( T ) );
		}
	}
}
