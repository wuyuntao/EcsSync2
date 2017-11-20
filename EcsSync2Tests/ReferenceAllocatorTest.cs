using EcsSync2.Fps;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EcsSync2.Tests
{
	[TestClass]
	public class ReferenceAllocatorTest
	{
		[TestMethod]
		public void TestAllocateAndRelease()
		{
			var allocator = new ReferencableAllocator( null, 2 );

			// #1
			var o1 = allocator.Allocate<MoveCharacterCommand>();
			o1.ComponentId = 2;
			o1.InputDirection = new Vector2D( 1, 1 );
			o1.InputMagnitude = 2;
			AssertReferencedCount( 1, o1 );

			o1.Release();
			AssertReferencedCount( 0, o1 );
			Assert.AreEqual( 0u, o1.ComponentId );
			Assert.AreEqual( Vector2D.Zero, o1.InputDirection );
			Assert.AreEqual( 0f, o1.InputMagnitude );

			// #2
			var o2 = allocator.Allocate( typeof( MoveCharacterCommand ) );
			AssertReferencedCount( 1, o2 );
			Assert.AreSame( o1, o2 );

			// #3
			var c1 = new MoveCharacterCommand()
			{
				ComponentId = 3,
				InputDirection = new Vector2D( 2, 2 ),
				InputMagnitude = 3,
			};
			var o3 = allocator.Allocate( c1 );
			AssertReferencedCount( 1, o3 );
			Assert.AreNotEqual( o1, o3 );

			o3.Release();
			AssertReferencedCount( 0, o3 );
			Assert.AreEqual( 0u, o3.ComponentId );
			Assert.AreEqual( Vector2D.Zero, o3.InputDirection );
			Assert.AreEqual( 0f, o3.InputMagnitude );
			o2.Release();

			// #4
			var c2 = new MoveCharacterCommand()
			{
				ComponentId = 4,
				InputDirection = new Vector2D( 3, 3 ),
				InputMagnitude = 4,
			};
			var o4 = (MoveCharacterCommand)allocator.Allocate( typeof( MoveCharacterCommand ), c2 );
			AssertReferencedCount( 1, o4 );
			Assert.AreEqual( 4u, o4.ComponentId );
		}

		void AssertReferencedCount(int expectedCount, IReferencable referencable)
		{
			Assert.AreEqual( expectedCount, referencable.ReferenceCounter.ReferencedCount );
		}
	}
}
