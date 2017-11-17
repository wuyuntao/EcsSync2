using EcsSync2;
using UnityEngine;

static class EcsSync2Extensions
{
	public static Vector3 ToUnityPos(this Vector2D v)
	{
		return new Vector3( v.X, 0, v.Y );
	}
}
