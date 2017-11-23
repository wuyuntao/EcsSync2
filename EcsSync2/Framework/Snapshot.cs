using EcsSync2.Fps;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EcsSync2
{

	[MessagePackObject]
	public class EntitySnapshot : Referencable
	{
		[Key( 10 )]
		public uint Id;

		[Key( 11 )]
		public IEntitySettings Settings;

		[Key( 12 )]
		public List<IComponentSnapshot> Components = new List<IComponentSnapshot>();

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

	[Union( 0, typeof( CharacterMotionControllerSnapshot ) )]
	[Union( 1, typeof( TransformSnapshot ) )]
	[Union( 2, typeof( ProcessControllerSnapshot ) )]
	[Union( 3, typeof( ConnectingSnapshot ) )]
	[Union( 4, typeof( ConnectedSnapshot ) )]
	[Union( 5, typeof( DisconnectedSnapshot ) )]
	public interface IComponentSnapshot : IReferencable
	{
	}

	public abstract class ComponentSnapshot : MessagePackReferencable, IComponentSnapshot
	{
		[Key( 10 )]
		public uint ComponentId;

		protected internal virtual bool IsApproximate(ComponentSnapshot other)
		{
			ReferenceCounter.Allocator.Simulator.Context.LogWarning( "Reflection IsApproximate {0}", this );

			var fields = GetType().GetFields( BindingFlags.Public | BindingFlags.Instance );
			foreach( var f in fields )
			{
				var attr = f.GetCustomAttribute( typeof( KeyAttribute ) );
				if( attr == null )
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
				var attr = f.GetCustomAttribute( typeof( KeyAttribute ) );
				if( attr == null )
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
