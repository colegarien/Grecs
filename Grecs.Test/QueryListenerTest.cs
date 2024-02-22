using Grecs;
using Grecs.Test.Stub;
using Xunit;

namespace Grecs.Test
{
    public class QueryListenerTest
    {
        private readonly EntityContext _context;

        public QueryListenerTest()
        {
            _context = new EntityContext();
        }


        [Fact]
        public void ListenToAdded_NoChangesReturnEmpty()
        {
            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = true;
            listener.ListenToChanged = false;
            listener.ListenToRemoved = false;

            var actual = listener.Gather();

            Assert.Empty(actual);
        }

        [Fact]
        public void ListenToAdded_EntityChangedBeforeListenerCreated()
        {
            var e = _context.CreateEntity();
            e.AddComponent(e.CreateComponent(typeof(ComponentA)));

            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = true;
            listener.ListenToChanged = false;
            listener.ListenToRemoved = false;

            var actual = listener.Gather();

            Assert.Empty(actual);
        }

        [Fact]
        public void ListenToAdded_EntityChangedIsNotInQuery()
        {
            var e = _context.CreateEntity();
            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = true;
            listener.ListenToChanged = false;
            listener.ListenToRemoved = false;

            e.AddComponent(e.CreateComponent(typeof(ComponentB)));

            var actual = listener.Gather();

            Assert.Empty(actual);
        }

        [Fact]
        public void ListenToAdded_EntityMeetsQueryButAddedComponentNotInQuery()
        {
            var e = _context.CreateEntity();
            e.AddComponent(e.CreateComponent(typeof(ComponentA)));

            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = true;
            listener.ListenToChanged = false;
            listener.ListenToRemoved = false;

            e.AddComponent(e.CreateComponent(typeof(ComponentB)));

            var actual = listener.Gather();

            Assert.Empty(actual);
        }

        [Fact]
        public void ListenToAdded_EntityAndComponentMeetsQuery_PullResult()
        {
            var e = _context.CreateEntity();
            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = true;
            listener.ListenToChanged = false;
            listener.ListenToRemoved = false;

            var componentA = e.CreateComponent(typeof(ComponentA));
            e.AddComponent(componentA);

            var actualFirst = listener.Gather();
            var actualSecond = listener.Gather();

            Assert.Same(e, actualFirst[0]);
            Assert.Empty(actualSecond);
        }



        [Fact]
        public void ListenToChanged_NoChangesReturnEmpty()
        {
            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = false;
            listener.ListenToChanged = true;
            listener.ListenToRemoved = false;

            var actual = listener.Gather();

            Assert.Empty(actual);
        }

        [Fact]
        public void ListenToChanged_EntityChangedBeforeListenerCreated()
        {
            var e = _context.CreateEntity();
            var c = (ComponentA)e.CreateComponent(typeof(ComponentA));
            e.AddComponent(c);

            c.Value = "So Cool";

            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = false;
            listener.ListenToChanged = true;
            listener.ListenToRemoved = false;

            var actual = listener.Gather();

            Assert.Empty(actual);
        }

        [Fact]
        public void ListenToChanged_EntityChangedIsNotInQuery()
        {
            var e = _context.CreateEntity();
            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = false;
            listener.ListenToChanged = true;
            listener.ListenToRemoved = false;

            var c = (ComponentB)e.CreateComponent(typeof(ComponentB));
            e.AddComponent(c);
            c.SomeNumber = 23;

            var actual = listener.Gather();

            Assert.Empty(actual);
        }

        [Fact]
        public void ListenToChanged_EntityAndComponentMeetsQueryButOnlyAdded()
        {
            var e = _context.CreateEntity();
            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = false;
            listener.ListenToChanged = true;
            listener.ListenToRemoved = false;

            var componentA = e.CreateComponent(typeof(ComponentA));
            e.AddComponent(componentA);

            var actual = listener.Gather();

            Assert.Empty(actual);
        }

        [Fact]
        public void ListenToChanged_EntityComponentValueChanged_PullResult()
        {
            var e = _context.CreateEntity();
            var c = (ComponentA)e.CreateComponent(typeof(ComponentA));
            e.AddComponent(c);

            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = false;
            listener.ListenToChanged = true;
            listener.ListenToRemoved = false;

            c.Value = "Too Cool";

            var actualFirst = listener.Gather();
            var actualSecond = listener.Gather();

            Assert.Same(e, actualFirst[0]);
            Assert.Empty(actualSecond);
        }

        [Fact]
        public void ListenToChanged_EntityComponentValueChangedToSameValue_PullResult()
        {
            var e = _context.CreateEntity();
            var c = (ComponentA)e.CreateComponent(typeof(ComponentA));
            e.AddComponent(c);

            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = false;
            listener.ListenToChanged = true;
            listener.ListenToRemoved = false;

            c.Value = "Too Cool";
            var actualFirst = listener.Gather();

            c.Value = "Too Cool";
            var actualSecond = listener.Gather();

            Assert.Same(e, actualFirst[0]);
            Assert.Empty(actualSecond);
        }

        [Fact]
        public void ListenToChanged_EntityComponentValueChangedToDiffertValue_PullResult()
        {
            var e = _context.CreateEntity();
            var c = (ComponentA)e.CreateComponent(typeof(ComponentA));
            e.AddComponent(c);

            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = false;
            listener.ListenToChanged = true;
            listener.ListenToRemoved = false;

            c.Value = "Too Cool";
            var actualFirst = listener.Gather();

            c.Value = "So Cool";
            var actualSecond = listener.Gather();

            Assert.Same(e, actualFirst[0]);
            Assert.Same(e, actualSecond[0]);
        }


        [Fact]
        public void ListenToRemoved_NothingRemoved()
        {
            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = false;
            listener.ListenToChanged = false;
            listener.ListenToRemoved = true;

            var actual = listener.Gather();

            Assert.Empty(actual);
        }

        [Fact]
        public void ListenToRemoved_ComponentRemovedBeforeListener()
        {
            var e = _context.CreateEntity();
            var c = (ComponentA)e.CreateComponent(typeof(ComponentA));
            e.AddComponent(c);

            e.RemoveComponent(c);

            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = false;
            listener.ListenToChanged = false;
            listener.ListenToRemoved = true;

            var actual = listener.Gather();

            Assert.Empty(actual);
        }

        [Fact]
        public void ListenToRemoved_ComponentRemovedButNotInQuery()
        {
            var e = _context.CreateEntity();
            var c = (ComponentA)e.CreateComponent(typeof(ComponentA));
            var c2 = (ComponentB)e.CreateComponent(typeof(ComponentB));
            e.AddComponent(c);
            e.AddComponent(c2);

            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = false;
            listener.ListenToChanged = false;
            listener.ListenToRemoved = true;

            e.RemoveComponent(c2);
            var actual = listener.Gather();

            Assert.Empty(actual);
        }

        [Fact]
        public void ListenToRemoved_ComponentRemovedAndEntityStillInQuery_PullResult()
        {
            var e = _context.CreateEntity();
            var c = (ComponentA)e.CreateComponent(typeof(ComponentA));
            var c2 = (ComponentB)e.CreateComponent(typeof(ComponentB));
            e.AddComponent(c);
            e.AddComponent(c2);

            var query = new EntityQuery().Or(typeof(ComponentA), typeof(ComponentB));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = false;
            listener.ListenToChanged = false;
            listener.ListenToRemoved = true;

            e.RemoveComponent(c);
            var actualFirst = listener.Gather();
            var actualSecond = listener.Gather();

            Assert.Same(e, actualFirst[0]);
            Assert.Empty(actualSecond);
        }

        [Fact]
        public void ListenToRemoved_ComponentRemovedAndEntityNotInQuery_PullResult()
        {
            var e = _context.CreateEntity();
            var c = (ComponentA)e.CreateComponent(typeof(ComponentA));
            var c2 = (ComponentB)e.CreateComponent(typeof(ComponentB));
            e.AddComponent(c);
            e.AddComponent(c2);

            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _context.CreateListener(query);
            listener.ListenToAdded = false;
            listener.ListenToChanged = false;
            listener.ListenToRemoved = true;

            e.RemoveComponent(c);
            var actualFirst = listener.Gather();
            var actualSecond = listener.Gather();

            Assert.Same(e, actualFirst[0]);
            Assert.Empty(actualSecond);
        }
    }
}
