
namespace EcsSync2.Examples
{
	class FakeEntityPawn : Entity.IContext, Interpolator.IContext
	{
		public FakeEntityPawn(Entity entity)
		{
			entity.Context = this;

			var interpolator = (Interpolator)entity.Components.Find( c => c is Interpolator );
			if( interpolator != null )
			{
				interpolator.Context = this;
			}
		}

		void Interpolator.IContext.SetPosition(Vector2D vector)
		{
		}
	}
}