using Grecs;
using Xunit;

namespace Grecs.Test
{
    public class EntityContextTest
    {
        private readonly EntityContext _context;


        public EntityContextTest()
        {
            _context = new EntityContext();
        }

        [Fact]
        public void CreateEntitiesWithUniqueIds()
        {
            var a = _context.CreateEntity();
            var b = _context.CreateEntity();
            var c = _context.CreateEntity();

            Assert.NotSame(a, b);
            Assert.NotSame(b, c);

            Assert.NotEqual(a.id, b.id);
            Assert.NotEqual(b.id, c.id);
        }

        [Fact]
        public void CreateEntitiesWithId()
        {
            var a = _context.CreateEntity(1);
            var b = _context.CreateEntity(2);
            var c = _context.CreateEntity(3);
            var d = _context.CreateEntity();

            Assert.NotSame(a, b);
            Assert.NotSame(b, c);
            Assert.NotSame(c, d);

            Assert.NotEqual(a.id, b.id);
            Assert.NotEqual(b.id, c.id);
            Assert.NotEqual(c.id, d.id);
            Assert.NotEqual(b.id, d.id);
            Assert.NotEqual(a.id, d.id);
        }

        [Fact]
        public void CreateForSameId()
        {
            var a = _context.CreateEntity(23);
            var b = _context.CreateEntity(23);

            Assert.Same(a, b);
            Assert.Equal(a.id, b.id);
        }

        [Fact]
        public void RemoveEntitiesTest()
        {
            var a = _context.CreateEntity();
            var b = _context.CreateEntity();
            var c = _context.CreateEntity();

            var bId = b.id;
            _context.DestroyEntity(b);
            var actual = _context.GetRecentlyDestroyed();

            var entities = _context.GetEntities();
            Assert.Contains(entities, e => e == a);
            Assert.DoesNotContain(entities, e => e == b);
            Assert.Contains(entities, e => e == c);

            Assert.Contains(actual, id => id == bId);
        }
    }
}
