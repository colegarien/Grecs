using Grecs;
using Grecs.Test.Stub;
using Xunit;

namespace Grecs.Test
{
    public class EntityTest
    {
        private readonly EntityContext _context;
        private readonly Entity _entity;

        private Entity? lastEventEntity;
        private IComponent? lastEventComponent;

        public EntityTest()
        {
            _context = new EntityContext();
            _entity = _context.CreateEntity();

            lastEventEntity = null;
            lastEventComponent = null;
        }

        internal void HandleComponentEvent(Entity entity, IComponent component)
        {
            lastEventEntity = entity;
            lastEventComponent = component;
        }

        [Fact]
        public void CreateComponentTest()
        {
            var one = _entity.CreateComponent<ComponentA>();
            var two = _entity.CreateComponent(typeof(ComponentA));
            Assert.IsType<ComponentA>(one);
            Assert.IsType<ComponentA>(two);
        }

        [Fact]
        public void AddComponentTest()
        {
            var component = _entity.CreateComponent<ComponentA>();
            _entity.AddComponent(component);

            Assert.True(_entity.HasComponent(typeof(ComponentA)));
            Assert.Same(component, _entity.GetComponents()[0]);
        }

        [Fact]
        public void AddComponent_MultipleTest()
        {
            var componentA = _entity.CreateComponent<ComponentA>();
            var componentB = _entity.CreateComponent<ComponentB>();
            _entity.AddComponent(componentA);
            _entity.AddComponent(componentB);

            Assert.Same(componentA, _entity.GetComponent(typeof(ComponentA)));
            Assert.Same(componentB, _entity.GetComponent(typeof(ComponentB)));
        }

        [Fact]
        public void RemoveComponentTest()
        {
            var componentA = _entity.CreateComponent<ComponentA>();
            var componentB = _entity.CreateComponent<ComponentB>();
            _entity.AddComponent(componentA);
            _entity.AddComponent(componentB);

            _entity.RemoveComponent(componentA);

            Assert.Null(_entity.GetComponent(typeof(ComponentA)));
            Assert.Same(componentB, _entity.GetComponent(typeof(ComponentB)));
            Assert.Same(componentB, _entity.GetComponents()[0]);
        }

        [Fact]
        public void RemoveAllComponents()
        {
            var componentA = _entity.CreateComponent<ComponentA>();
            var componentB = _entity.CreateComponent<ComponentB>();
            _entity.AddComponent(componentA);
            _entity.AddComponent(componentB);

            _entity.RemoveAllComponents();

            Assert.Null(_entity.GetComponent(typeof(ComponentA)));
            Assert.Null(_entity.GetComponent(typeof(ComponentB)));
        }

        [Fact]
        public void TestComponetAddedEvent()
        {
            _entity.OnComponentAdded += HandleComponentEvent;
            var componentA = _entity.CreateComponent<ComponentA>();

            _entity.AddComponent(componentA);

            Assert.Same(_entity, lastEventEntity);
            Assert.Same(componentA, lastEventComponent);
        }

        [Fact]
        public void TestComponetRemovedEvent()
        {
            _entity.OnComponentRemoved += HandleComponentEvent;
            var componentA = _entity.CreateComponent<ComponentA>();

            _entity.AddComponent(componentA);
            _entity.RemoveComponent(componentA);

            Assert.Same(_entity, lastEventEntity);
            Assert.Same(componentA, lastEventComponent);
        }
    }
}
