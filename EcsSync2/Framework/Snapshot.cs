﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EcsSync2
{

	[ProtoContract]
	public class EntitySnapshot : Referencable
	{
		[ProtoMember( 11 )]
		public uint Id;

		[ProtoMember( 12 )]
		public EntitySettings Settings;

		[ProtoMember( 13 )]
		public List<ComponentSnapshot> Components = new List<ComponentSnapshot>();

		protected override void OnAllocate()
		{
			base.OnAllocate();

			foreach( var c in Components )
				this.Allocate( c );
		}

		protected override void OnReset()
		{
			foreach( ComponentSnapshot c in Components )
				c.Release();

			Components.Clear();

			base.OnReset();
		}
	}

	[ProtoContract]
	[ProtoInclude( 21, typeof( TransformSnapshot ) )]
	[ProtoInclude( 22, typeof( AnimatorSnapshot ) )]
	public abstract class ComponentSnapshot : SerializableReferencable
	{
		[ProtoMember( 11 )]
		public uint ComponentId;

		protected internal virtual bool IsApproximate(ComponentSnapshot other)
		{
			ReferenceCounter.Allocator.Simulator.Context.LogWarning( "Reflection IsApproximate {0}", this );

			var fields = GetType().GetFields( BindingFlags.Public | BindingFlags.Instance );
			foreach( var f in fields )
			{
				var isMember = f.GetCustomAttributes( true ).Any( a => a is ProtoMemberAttribute );
				if( !isMember )
					continue;

				if( f.FieldType == typeof( Vector2D ) )
				{
					var value1 = (Vector2D)f.GetValue( this );
					var value2 = (Vector2D)f.GetValue( other );

					if( !IsApproximate( value1.X, value2.X ) || !IsApproximate( value1.Y, value2.Y ) )
					{
						//ReferenceCounter.Allocator.Simulator.Context.Log( "IsApproximate {0}.{1}", GetType(), f );
						return false;
					}
				}
				else if( f.FieldType == typeof( float ) )
				{
					var value1 = (float)f.GetValue( this );
					var value2 = (float)f.GetValue( other );

					if( !IsApproximate( value1, value2 ) )
					{
						//ReferenceCounter.Allocator.Simulator.Context.Log( "IsApproximate {0}.{1}", GetType(), f );
						return false;
					}
				}
				else
				{
					var value1 = f.GetValue( this );
					var value2 = f.GetValue( other );

					if( !value1.Equals( value2 ) )
					{
						//ReferenceCounter.Allocator.Simulator.Context.Log( "IsApproximate {0}.{1}", GetType(), f );
						return false;
					}
				}
			}

			return true;
		}

		protected static bool IsApproximate(Vector2D value1, Vector2D value2, float error = 1e-6f)
		{
			return IsApproximate( value1.X, value2.X, error ) && IsApproximate( value1.Y, value2.Y, error );
		}

		protected static bool IsApproximate(float value1, float value2, float error = 1e-6f)
		{
			return Math.Abs( value1 - value2 ) <= error;
		}

		protected static bool IsApproximate(uint value1, uint value2, uint error = 0)
		{
			return ( value1 > value2 ? value1 - value2 : value2 - value1 ) <= error;
		}

		protected internal virtual ComponentSnapshot Interpolate(ComponentSnapshot other, float factor)
		{
			return Clone();
		}

		public virtual ComponentSnapshot Clone()
		{
			ReferenceCounter.Allocator.Simulator.Context.LogWarning( "Reflection Clone {0}", this );

			var s = this.Allocate( GetType() );

			var fields = GetType().GetFields( BindingFlags.Public | BindingFlags.Instance );
			foreach( var f in fields )
			{
				var isMember = f.GetCustomAttributes( true ).Any( a => a is ProtoMemberAttribute );
				if( !isMember )
					continue;

				f.SetValue( s, f.GetValue( this ) );
			}

			return (ComponentSnapshot)s;
		}

		protected internal virtual ComponentSnapshot Extrapolate(uint time, uint extrapolateTime)
		{
			return Clone();
		}

		protected internal virtual ComponentSnapshot Interpolate(uint time, ComponentSnapshot targetSnapshot, uint targetTime, uint interpolateTime)
		{
			return Clone();
		}
	}
}
