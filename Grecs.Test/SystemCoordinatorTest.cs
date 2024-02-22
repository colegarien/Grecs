using Grecs;
using Grecs.Test.Stub;
using Xunit;

namespace Grecs.Test
{
    public class SystemCoordinatorTest
    {
        private readonly EntityContext _context;
        private readonly SystemCoordinator _coordinator;
        private const float DELTA_TIME = 0.16f;

        public SystemCoordinatorTest()
        {
            _context = new EntityContext();
            _coordinator = new SystemCoordinator();
        }

        [Fact]
        public void TickSystemOrdering()
        {
            var systemA = new NumberIncrementTickSystem();
            var systemB = new NumberIncrementTickSystem();
            var systemC = new NumberIncrementTickSystem();

            _coordinator.Add(systemA)
                .Add(systemB)
                .Add(systemC);

            _coordinator.Execute(DELTA_TIME, _context);

            Assert.True(systemA.InstanceTick < systemB.InstanceTick);
            Assert.True(systemB.InstanceTick < systemC.InstanceTick);
        }

        [Fact]
        public void TickSystemUpdatingComponents()
        {
            var entity = _context.CreateEntity();
            var component = (ComponentB)entity.CreateComponent<ComponentB>();
            component.SomeNumber = 1;
            entity.AddComponent(component);

            _coordinator.Add(new ComponentBIterateTickSystem());

            _coordinator.Execute(DELTA_TIME, _context);

            Assert.Equal(2, ((ComponentB)entity.GetComponent(typeof(ComponentB))).SomeNumber);
        }

        [Fact]
        public void TickSystemUpdatingComponents_MultipleSystems()
        {
            var entity = _context.CreateEntity();
            var component = (ComponentB)entity.CreateComponent<ComponentB>();
            component.SomeNumber = 1;
            entity.AddComponent(component);

            _coordinator.Add(new ComponentBIterateTickSystem())
                .Add(new ComponentBIterateTickSystem());

            _coordinator.Execute(DELTA_TIME, _context);

            Assert.Equal(3, ((ComponentB)entity.GetComponent(typeof(ComponentB))).SomeNumber);
        }

        [Fact]
        public void TickSystemUpdatingComponents_MultipleTicks()
        {
            var entity = _context.CreateEntity();
            var component = (ComponentB)entity.CreateComponent<ComponentB>();
            component.SomeNumber = 1;
            entity.AddComponent(component);

            _coordinator.Add(new ComponentBIterateTickSystem());

            _coordinator.Execute(DELTA_TIME, _context);
            _coordinator.Execute(DELTA_TIME, _context);

            Assert.Equal(3, ((ComponentB)entity.GetComponent(typeof(ComponentB))).SomeNumber);
        }

        [Fact]
        public void TickSystemAddRemoveComponents()
        {
            var entityA = _context.CreateEntity();
            entityA.AddComponent(entityA.CreateComponent(typeof(ComponentA)));

            var entityB = _context.CreateEntity();
            entityB.AddComponent(entityB.CreateComponent(typeof(ComponentB)));

            _coordinator.Add(new SwapComponentTickSystem());

            _coordinator.Execute(DELTA_TIME, _context);
            Assert.False(entityA.HasComponent(typeof(ComponentA)));
            Assert.True(entityA.HasComponent(typeof(ComponentB)));
            Assert.True(entityB.HasComponent(typeof(ComponentA)));
            Assert.False(entityB.HasComponent(typeof(ComponentB)));

            _coordinator.Execute(DELTA_TIME, _context);
            Assert.True(entityA.HasComponent(typeof(ComponentA)));
            Assert.False(entityA.HasComponent(typeof(ComponentB)));
            Assert.False(entityB.HasComponent(typeof(ComponentA)));
            Assert.True(entityB.HasComponent(typeof(ComponentB)));
        }

        [Fact]
        public void TickSystemAddRemoveEntities()
        {
            var entity1 = _context.CreateEntity();
            entity1.AddComponent(entity1.CreateComponent(typeof(ComponentA)));
            var entity2 = _context.CreateEntity();
            entity2.AddComponent(entity2.CreateComponent(typeof(ComponentA)));
            var entity3 = _context.CreateEntity();
            entity3.AddComponent(entity3.CreateComponent(typeof(ComponentA)));

            var queryA = new EntityQuery().And(typeof(ComponentA));
            var queryB = new EntityQuery().And(typeof(ComponentB));

            _coordinator.Add(new AddRemoveEntityTickSystem());

            _coordinator.Execute(DELTA_TIME, _context);
            Assert.Equal(2, _context.GetEntities(queryA).Length);
            Assert.Single(_context.GetEntities(queryB));

            _coordinator.Execute(DELTA_TIME, _context);
            Assert.Single(_context.GetEntities(queryA));
            Assert.Equal(2, _context.GetEntities(queryB).Length);

            _coordinator.Execute(DELTA_TIME, _context);
            Assert.Empty(_context.GetEntities(queryA));
            Assert.Equal(3, _context.GetEntities(queryB).Length);

            _coordinator.Execute(DELTA_TIME, _context);
            Assert.Empty(_context.GetEntities(queryA));
            Assert.Equal(3, _context.GetEntities(queryB).Length);
        }

        [Fact]
        public void TriggerSystemNotTriggered()
        {
            var system = new CountingTriggerSystem(_context);
            _coordinator.Add(system);

            _coordinator.Execute(DELTA_TIME, _context);

            Assert.Equal(0, system.TriggerCount);
        }

        [Fact]
        public void TriggerSystemSimpleTrigger()
        {
            var entity = _context.CreateEntity();
            var system = new CountingTriggerSystem(_context);
            _coordinator.Add(system);


            entity.AddComponent(entity.CreateComponent(typeof(ComponentA)));
            _coordinator.Execute(DELTA_TIME, _context);
            // no changes to any entities
            _coordinator.Execute(DELTA_TIME, _context);

            Assert.Equal(1, system.TriggerCount);
        }

        [Fact]
        public void TriggerSystemOrdering()
        {
            var entity = _context.CreateEntity();
            var systemA = new CountingTriggerSystem(_context);
            var systemB = new CountingTriggerSystem(_context);
            var systemC = new CountingTriggerSystem(_context);

            _coordinator.Add(systemA)
                .Add(systemB)
                .Add(systemC);

            entity.AddComponent(entity.CreateComponent(typeof(ComponentA)));
            _coordinator.Execute(DELTA_TIME, _context);

            Assert.True(systemA.InstanceCount < systemB.InstanceCount);
            Assert.True(systemB.InstanceCount < systemC.InstanceCount);
        }
    }
}
