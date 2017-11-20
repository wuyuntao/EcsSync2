using EcsSync2.Fps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EcsSync2.Tests
{
	[TestClass]
	public class TimelineTest
	{
		[TestMethod]
		public void TestTimeline()
		{
			var deltaTime = Configuration.SimulationDeltaTime;

			var allocator = new ReferencableAllocator( null );
			var timeline = new Timeline( allocator, 2 );
			Assert.AreEqual( 2, timeline.Capacity );
			Assert.AreEqual( 0, timeline.Count );

			timeline.Add( deltaTime * 1, AllocateTransformSnapshot( allocator, new Vector2D( 1, 0 ) ) );
			timeline.Add( deltaTime * 2, AllocateTransformSnapshot( allocator, new Vector2D( 2, 0 ) ) );
			Assert.AreEqual( 2, timeline.Capacity );
			Assert.AreEqual( 2, timeline.Count );

			timeline.Add( deltaTime * 3, AllocateTransformSnapshot( allocator, new Vector2D( 3, 0 ) ) );
			timeline.Add( deltaTime * 4, AllocateTransformSnapshot( allocator, new Vector2D( 4, 0 ) ) );
			Assert.AreEqual( 4, timeline.Capacity );
			Assert.AreEqual( 4, timeline.Count );

			var snapshot1 = (TransformSnapshot)timeline.Find( deltaTime );
			Assert.AreEqual( snapshot1.Position, new Vector2D( 1, 0 ) );

			var snapshot2 = (TransformSnapshot)timeline.Find( (uint)Math.Round( deltaTime * 1.5f ) );
			Assert.AreEqual( snapshot2.Position, new Vector2D( 1, 0 ) );

			var count1 = timeline.RemoveBefore( 50 );
			Assert.AreEqual( 0, count1 );
			Assert.AreEqual( 4, timeline.Count );

			var count2 = timeline.RemoveBefore( 150 );
			Assert.AreEqual( 0, count2 );
			Assert.AreEqual( 4, timeline.Count );

			var count3 = timeline.RemoveBefore( 250 );
			Assert.AreEqual( 1, count3 );
			Assert.AreEqual( 3, timeline.Count );

			var count4 = timeline.RemoveBefore( 550 );
			Assert.AreEqual( 2, count4 );
			Assert.AreEqual( 1, timeline.Count );

			timeline.Clear();
			Assert.AreEqual( 4, timeline.Capacity );
			Assert.AreEqual( 0, timeline.Count );
		}

		static TransformSnapshot AllocateTransformSnapshot(ReferencableAllocator allocator, Vector2D position)
		{
			var snapshot = allocator.Allocate<TransformSnapshot>();
			snapshot.Position = position;
			return snapshot;
		}
	}
}
