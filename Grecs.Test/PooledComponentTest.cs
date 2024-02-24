using Grecs.Test.Stub;

namespace Grecs.Test
{
    public class PooledComponentTest
    {
        private readonly EntityContext _context;

        public PooledComponentTest()
        {
            _context = new EntityContext();
        }


        [Fact]
        public void TestStaticPooledComponentStorage()
        {
            /// TestGettingBackTheSameInstance()

            // Arrange
            var entityOne = _context.CreateEntity();
            var componentOne = PooledComponentA.GetInstance();
            componentOne.Value = "From One";
            entityOne.AddComponent(componentOne);

            var entityTwo = _context.CreateEntity();
            var componentTwo = PooledComponentA.GetInstance();
            componentTwo.Value = "From Two";
            entityTwo.AddComponent(componentTwo);

            var componentThree = PooledComponentA.GetInstance();

            // Act
            _context.DestroyEntity(entityOne);
            _context.DestroyEntity(entityTwo);

            var fromPoolOne = PooledComponentA.GetInstance();
            var fromPoolTwo = PooledComponentA.GetInstance();
            var fromPoolThree = PooledComponentA.GetInstance();

            // Assert
            Assert.Same(fromPoolOne, componentOne);
            Assert.Same(fromPoolTwo, componentTwo);
            Assert.Equal("From One", fromPoolOne.Value);
            Assert.Equal("From Two", fromPoolTwo.Value);
            Assert.Equal("", componentThree.Value);
            Assert.Equal("", fromPoolThree.Value);

            // flush pools to clean up for other tests
            PooledComponentA.FlushInstances();
            PooledComponentB.FlushInstances();


        /// TestGettingBackTheSameInstance_TwoDifferentObjects()
            // Arrange
            var entityA = _context.CreateEntity();
            var componentA = PooledComponentA.GetInstance();
            componentA.Value = "From A";
            entityA.AddComponent(componentA);

            var entityB = _context.CreateEntity();
            var componentB = PooledComponentB.GetInstance();
            componentB.SomeNumber = 3;
            entityB.AddComponent(componentB);

            // Act
            _context.DestroyEntity(entityA);
            var fromPoolA = PooledComponentA.GetInstance();
            _context.DestroyEntity(entityB);
            var fromPoolB = PooledComponentB.GetInstance();

            // Assert
            Assert.Same(fromPoolA, componentA);
            Assert.Same(fromPoolB, componentB);
            Assert.Equal("From A", componentA.Value);
            Assert.Equal(3, fromPoolB.SomeNumber);

            // flush pools to clean up for other tests
            PooledComponentA.FlushInstances();
            PooledComponentB.FlushInstances();


        /// TestEntityRemoveAndCreateComponent
            // Arrange
            entityA = _context.CreateEntity();

            componentA = (PooledComponentA)entityA.CreateComponent<PooledComponentA>();
            componentA.Value = "From A";
            entityA.AddComponent(componentA);

            componentB = (PooledComponentB)entityA.CreateComponent(typeof(PooledComponentB));
            componentB.SomeNumber = 3;
            entityA.AddComponent(componentB);

            // Act
            entityA.RemoveComponent(componentA);
            fromPoolA = PooledComponentA.GetInstance();
            entityA.RemoveComponent(componentB);
            fromPoolB = PooledComponentB.GetInstance();

            // Assert
            Assert.Same(fromPoolA, componentA);
            Assert.Same(fromPoolB, componentB);
            Assert.Equal("From A", componentA.Value);
            Assert.Equal(3, fromPoolB.SomeNumber);

            // flush pools to clean up for other tests
            PooledComponentA.FlushInstances();
            PooledComponentB.FlushInstances();

            /// TEST FLUSH

            // Arrange
            entityA = _context.CreateEntity();

            componentA = (PooledComponentA)entityA.CreateComponent<PooledComponentA>();
            componentA.Value = "From A";
            entityA.AddComponent(componentA);
            entityA.RemoveComponent(componentA);

            // Act
            PooledComponentA.FlushInstances();
            fromPoolA = PooledComponentA.GetInstance();

            // Assert
            Assert.NotSame(fromPoolA, componentA);
            Assert.Equal("From A", componentA.Value);
            Assert.Equal("", fromPoolA.Value);

            // flush pools to clean up for other tests
            PooledComponentA.FlushInstances();
            PooledComponentB.FlushInstances();


            /// TestQueryingForPooledComponents
            // Arrange
            entityA = _context.CreateEntity();

            componentA = (PooledComponentA)entityA.CreateComponent<PooledComponentA>();
            componentA.Value = "From A";
            entityA.AddComponent(componentA);
            entityA.RemoveComponent(componentA);

            componentB = (PooledComponentB)entityA.CreateComponent(typeof(PooledComponentB));
            componentB.SomeNumber = 32;
            entityA.AddComponent(componentB);
            entityA.RemoveComponent(componentB);

            // Act
            var theAs = _context.GetEntities((new EntityQuery()).Or(typeof(PooledComponentA), typeof(PooledComponentB)));

            // Assert
            Assert.Empty(theAs);
            Assert.Same(componentA, PooledComponentA.GetInstance());
            Assert.NotSame(componentA, PooledComponentA.GetInstance());
            Assert.Same(componentB, PooledComponentB.GetInstance());
            Assert.NotSame(componentB, PooledComponentB.GetInstance());

            // flush pools to clean up for other tests
            PooledComponentA.FlushInstances();
            PooledComponentB.FlushInstances();
        }
    }
}
