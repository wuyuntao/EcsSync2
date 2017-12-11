namespace EcsSync2.Fps
{
	static class MathUtils
	{
		public static float Clamp(float value, float minValue, float maxValue)
		{
			if( value < minValue )
				return minValue;

			if( value > maxValue )
				return maxValue;

			return value;
		}

		public static float Lerp(float a, float b, float t)
		{
			return a + ( b - a ) * Clamp( t, 0f, 1f );
		}

		public static Vector2D Lerp(Vector2D a, Vector2D b, float t)
		{
			t = Clamp( t, 0f, 1f );
			return new Vector2D( a.X + ( b.X - a.X ) * t, a.Y + ( b.Y - a.Y ) * t );
		}
	}
}
