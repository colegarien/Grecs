using Grecs;
using Grecs.Test.Stub;
using Xunit;

namespace Grecs.Test
{
    public class EntityQueryTest
    {
        private readonly EntityContext _context;
        private readonly Entity _entityAB;
        private readonly Entity _entityA;
        private readonly Entity _entityB;


        public EntityQueryTest()
        {
            _context = new EntityContext();
            _entityAB = _context.CreateEntity();
            _entityAB.AddComponent(_entityAB.CreateComponent<ComponentA>());
            _entityAB.AddComponent(_entityAB.CreateComponent<ComponentB>());

            _entityA = _context.CreateEntity();
            _entityA.AddComponent(_entityA.CreateComponent<ComponentA>());

            _entityB = _context.CreateEntity();
            _entityB.AddComponent(_entityB.CreateComponent<ComponentB>());
        }

        [Fact]
        public void EmptyQueryTest()
        {
            var query = new EntityQuery();

            var actual = _context.GetEntities(query);

            Assert.Empty(actual);
        }

        [Fact]
        public void SimpleAndsTest_NoMatches()
        {
            var query = new EntityQuery();
            query.And(typeof(ComponentC));

            var actual = _context.GetEntities(query);

            Assert.Empty(actual);
        }

        [Fact]
        public void SimpleAndsTest()
        {
            var queryA = new EntityQuery();
            queryA.And(typeof(ComponentA));

            var queryB = new EntityQuery();
            queryB.And(typeof(ComponentB));

            var queryAB = new EntityQuery();
            queryAB.And(typeof(ComponentA), typeof(ComponentB));

            var actualA = _context.GetEntities(queryA);
            var actualB = _context.GetEntities(queryB);
            var actualAB = _context.GetEntities(queryAB);

            Assert.Equal(2, actualA.Length);
            Assert.Equal(2, actualB.Length);
            Assert.Single(actualAB);
        }


        [Fact]
        public void SimpleOrsTest_NoMatches()
        {
            var query = new EntityQuery();
            query.Or(typeof(ComponentC));

            var actual = _context.GetEntities(query);

            Assert.Empty(actual);
        }

        [Fact]
        public void SimpleOrsTest()
        {
            var queryA = new EntityQuery();
            queryA.Or(typeof(ComponentA));

            var queryB = new EntityQuery();
            queryB.Or(typeof(ComponentB));

            var queryAB = new EntityQuery();
            queryAB.Or(typeof(ComponentA), typeof(ComponentB));

            var actualA = _context.GetEntities(queryA);
            var actualB = _context.GetEntities(queryB);
            var actualAB = _context.GetEntities(queryAB);

            Assert.Equal(2, actualA.Length);
            Assert.Equal(2, actualB.Length);
            Assert.Equal(3, actualAB.Length);
        }


        [Fact]
        public void NoneWithoutAndOrTest_NoMatches()
        {
            var query = new EntityQuery();
            query.None(typeof(ComponentC));

            var actual = _context.GetEntities(query);

            Assert.Empty(actual);
        }

        [Fact]
        public void SimpleAndNoneTest()
        {
            var queryFilterAll = new EntityQuery();
            queryFilterAll.And(typeof(ComponentA))
                .None(typeof(ComponentA));

            var queryFilterOne = new EntityQuery();
            queryFilterOne.And(typeof(ComponentB))
                .None(typeof(ComponentA));

            var queryFilterNone = new EntityQuery();
            queryFilterNone.And(typeof(ComponentA), typeof(ComponentB))
                .None(typeof(ComponentC));

            var actualAll = _context.GetEntities(queryFilterAll);
            var actualOne = _context.GetEntities(queryFilterOne);
            var actualNone = _context.GetEntities(queryFilterNone);

            Assert.Empty(actualAll);
            Assert.Single(actualOne);
            Assert.Single(actualNone);
        }

        [Fact]
        public void SimpleOrNoneTest()
        {
            var queryFilterAll = new EntityQuery();
            queryFilterAll.Or(typeof(ComponentA))
                .None(typeof(ComponentA));

            var queryFilterOne = new EntityQuery();
            queryFilterOne.Or(typeof(ComponentB))
                .None(typeof(ComponentA));

            var queryFilterNone = new EntityQuery();
            queryFilterNone.Or(typeof(ComponentA), typeof(ComponentB))
                .None(typeof(ComponentC));

            var actualAll = _context.GetEntities(queryFilterAll);
            var actualOne = _context.GetEntities(queryFilterOne);
            var actualNone = _context.GetEntities(queryFilterNone);

            Assert.Empty(actualAll);
            Assert.Single(actualOne);
            Assert.Equal(3, actualNone.Length);
        }

        [Fact]
        public void AndOrTest()
        {
            var query = new EntityQuery();
            query.And(typeof(ComponentA))
                .Or(typeof(ComponentB));

            var actual = _context.GetEntities(query);

            Assert.Single(actual);
        }

        [Fact]
        public void MatchesEmptyTest()
        {
            var query = new EntityQuery();

            var actualA = query.Matches(_entityA);
            var actualB = query.Matches(_entityB);
            var actualAB = query.Matches(_entityAB);

            Assert.False(actualA);
            Assert.False(actualB);
            Assert.False(actualAB);
        }

        [Fact]
        public void MatchesOrTest()
        {
            var query = new EntityQuery()
                .Or(typeof(ComponentA), typeof(ComponentC));

            var actualA = query.Matches(_entityA);
            var actualB = query.Matches(_entityB);
            var actualAB = query.Matches(_entityAB);

            Assert.True(actualA);
            Assert.False(actualB);
            Assert.True(actualAB);
        }

        [Fact]
        public void MatchesAndTest()
        {
            var query = new EntityQuery()
                .And(typeof(ComponentA), typeof(ComponentB));

            var actualA = query.Matches(_entityA);
            var actualB = query.Matches(_entityB);
            var actualAB = query.Matches(_entityAB);

            Assert.False(actualA);
            Assert.False(actualB);
            Assert.True(actualAB);
        }

        [Fact]
        public void MatchesOrNoneTest()
        {
            var query = new EntityQuery()
                .Or(typeof(ComponentA), typeof(ComponentB))
                .None(typeof(ComponentB));

            var actualA = query.Matches(_entityA);
            var actualB = query.Matches(_entityB);
            var actualAB = query.Matches(_entityAB);

            Assert.True(actualA);
            Assert.False(actualB);
            Assert.False(actualAB);
        }

        [Fact]
        public void MatchesAndNoneTest()
        {
            var query = new EntityQuery()
                .And(typeof(ComponentA), typeof(ComponentB))
                .None(typeof(ComponentA));

            var actualA = query.Matches(_entityA);
            var actualB = query.Matches(_entityB);
            var actualAB = query.Matches(_entityAB);

            Assert.False(actualA);
            Assert.False(actualB);
            Assert.False(actualAB);
        }

        [Fact]
        public void MatchesAndOrTest()
        {
            var query = new EntityQuery()
                .And(typeof(ComponentA))
                .Or(typeof(ComponentB))
                .None(typeof(ComponentC));

            var actualA = query.Matches(_entityA);
            var actualB = query.Matches(_entityB);
            var actualAB = query.Matches(_entityAB);

            Assert.False(actualA);
            Assert.False(actualB);
            Assert.True(actualAB);
        }
    }
}
