using EcsSync2;
using UnityEngine;

static class EcsSync2Extensions
{
	public static Vector2 AsUnity2(this Vector2D v)
	{
		return new Vector2( v.X, v.Y );
	}

	public static Vector2 AsUnity3(this Vector2D v)
	{
		return new Vector3( v.X, v.Y, 0 );
	}

	public static Vector2D AsEcsSync2(this Vector2 v)
	{
		return new Vector2D( v.x, v.y );
	}

	public static Vector2D AsEcsSync2(this Vector3 v)
	{
		return new Vector2D( v.x, v.y );
	}
}
