using Grecs;
using Grecs.Test.Stub;
using Xunit;

namespace Grecs.Test
{
    public class AttachDetachTest
    {
        private readonly EntityContext _contextA;
        private readonly EntityContext _contextB;

        public AttachDetachTest()
        {
            _contextA = new EntityContext();
            _contextB = new EntityContext();
        }


        //
        // Attach Tests
        //

        [Fact]
        public void AttachTest_AttachAlreadyAttached_Nop()
        {
            var a = _contextA.CreateEntity();
            Assert.Single(_contextA.GetEntities());

            _contextA.AttachEntity(a);
            Assert.Single(_contextA.GetEntities());

            _contextA.AttachEntity(a);
            Assert.Single(_contextA.GetEntities());
        }

        [Fact]
        public void AttachTest_AttachNewEntityWithConflictingId_AddEntityAndGiveNewId()
        {
            var a = _contextA.CreateEntity();
            // create entity in separate context to generate a "conflicting" id
            var b = _contextB.CreateEntity(a.id);

            _contextA.AttachEntity(b);

            Assert.Equal(2, _contextA.GetEntities().Length);
            Assert.NotEqual(a.id, b.id);
        }

        [Fact]
        public void AttachTest_AttachedEntityWorksLikeCreatedEntity_PullsInComponentChangesAndListeners()
        {
            // Arrange
            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _contextA.CreateListener(query);
            listener.ListenToAdded = true;
            listener.ListenToChanged = true;
            listener.ListenToRemoved = true;

            var a = _contextA.CreateEntity();
            var b = _contextB.CreateEntity();
            _contextA.AttachEntity(b);

            var emptyGet = listener.Gather();
            Assert.Empty(emptyGet);

            a.AddComponent(a.CreateComponent(typeof(ComponentA)));
            b.AddComponent(b.CreateComponent(typeof(ComponentA)));

            var actualAdd = listener.Gather();
            Assert.Equal(2, actualAdd.Length);
            emptyGet = listener.Gather();
            Assert.Empty(emptyGet);

            var aComponent = (ComponentA)a.GetComponent(typeof(ComponentA));
            var bComponent = (ComponentA)b.GetComponent(typeof(ComponentA));
            aComponent.Value = "New Value For A";
            bComponent.Value = "New Value For B";

            var actualChanged = listener.Gather();
            Assert.Equal(2, actualChanged.Length);
            emptyGet = listener.Gather();
            Assert.Empty(emptyGet);

            a.RemoveComponent(aComponent);
            b.RemoveComponent(bComponent);

            var actualRemoved = listener.Gather();
            Assert.Equal(2, actualRemoved.Length);
            emptyGet = listener.Gather();
            Assert.Empty(emptyGet);
        }


        //
        // Detach Tests
        //

        [Fact]
        public void DetachTest_DetachNonExistentEntity_Nop()
        {
            _contextA.CreateEntity();
            _contextA.CreateEntity();

            var randomEntity = _contextB.CreateEntity();
            Assert.Equal(2, _contextA.GetEntities().Length);
            Assert.Single(_contextB.GetEntities());

            _contextA.DetachEntity(randomEntity);

            Assert.Equal(2, _contextA.GetEntities().Length);
            Assert.Single(_contextB.GetEntities());
        }

        [Fact]
        public void DetachTest_DetachAlreadyDetached_Nop()
        {
            _contextA.CreateEntity();
            var b = _contextA.CreateEntity();

            Assert.Equal(2, _contextA.GetEntities().Length);
            _contextA.DetachEntity(b);
            Assert.Single(_contextA.GetEntities());
        }

        [Fact]
        public void DetachTest_DetachedEntityNotManipulate_HasAllComponentsStill()
        {
            var a = _contextA.CreateEntity();

            var componentA = (ComponentA)a.CreateComponent(typeof(ComponentA));
            var componentB = (ComponentB)a.CreateComponent(typeof(ComponentB));
            var componentC = (ComponentC)a.CreateComponent(typeof(ComponentC));

            a.AddComponent(componentA);
            a.AddComponent(componentB);
            a.AddComponent(componentC);

            componentA.Value = "an value!";
            componentB.SomeNumber = 62;
            componentC.FloatyFloat = 92.5f;

            _contextA.DetachEntity(a);

            // values on reference same
            Assert.Equal("an value!", componentA.Value);
            Assert.Equal(62, componentB.SomeNumber);
            Assert.Equal(92.5f, componentC.FloatyFloat);

            // values within entity are same
            Assert.Equal("an value!", ((ComponentA)a.GetComponent(typeof(ComponentA))).Value);
            Assert.Equal(62, ((ComponentB)a.GetComponent(typeof(ComponentB))).SomeNumber);
            Assert.Equal(92.5f, ((ComponentC)a.GetComponent(typeof(ComponentC))).FloatyFloat);
        }

        [Fact]
        public void DetachTest_DettachedEntityInQueries_DoesntPullInComponentChangesAndListeners()
        {
            // Arrange
            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _contextA.CreateListener(query);
            listener.ListenToAdded = true;
            listener.ListenToChanged = true;
            listener.ListenToRemoved = true;

            var a = _contextA.CreateEntity();
            var b = _contextA.CreateEntity();

            // Check functions with entity b
            var emptyGet = listener.Gather();
            Assert.Empty(emptyGet);

            a.AddComponent(a.CreateComponent(typeof(ComponentA)));
            b.AddComponent(b.CreateComponent(typeof(ComponentA)));

            var actualAdd = listener.Gather();
            Assert.Equal(2, actualAdd.Length);
            emptyGet = listener.Gather();
            Assert.Empty(emptyGet);

            var aComponent = (ComponentA)a.GetComponent(typeof(ComponentA));
            var bComponent = (ComponentA)b.GetComponent(typeof(ComponentA));
            aComponent.Value = "New Value For A";
            bComponent.Value = "New Value For B";

            var actualChanged = listener.Gather();
            Assert.Equal(2, actualChanged.Length);
            emptyGet = listener.Gather();
            Assert.Empty(emptyGet);

            a.RemoveComponent(aComponent);
            b.RemoveComponent(bComponent);

            var actualRemoved = listener.Gather();
            Assert.Equal(2, actualRemoved.Length);
            emptyGet = listener.Gather();
            Assert.Empty(emptyGet);

            // Check functions without entity b
            _contextA.DetachEntity(b);

            emptyGet = listener.Gather();
            Assert.Empty(emptyGet);

            a.AddComponent(a.CreateComponent(typeof(ComponentA)));
            b.AddComponent(b.CreateComponent(typeof(ComponentA)));

            actualAdd = listener.Gather();
            Assert.Single(actualAdd);
            emptyGet = listener.Gather();
            Assert.Empty(emptyGet);

            aComponent = (ComponentA)a.GetComponent(typeof(ComponentA));
            bComponent = (ComponentA)b.GetComponent(typeof(ComponentA));
            aComponent.Value = "Double New Value For A";
            bComponent.Value = "Double New Value For B";

            actualChanged = listener.Gather();
            Assert.Single(actualChanged);
            emptyGet = listener.Gather();
            Assert.Empty(emptyGet);

            a.RemoveComponent(aComponent);
            b.RemoveComponent(bComponent);

            actualRemoved = listener.Gather();
            Assert.Single(actualRemoved);
            emptyGet = listener.Gather();
            Assert.Empty(emptyGet);
        }

        [Fact]
        public void DetachTest_AddEntityWithSameIdAsDetached_DoesntPullNewEntityInComponentChangesAndListeners()
        {
            // Arrange
            var query = new EntityQuery().And(typeof(ComponentA));
            var listener = _contextA.CreateListener(query);
            listener.ListenToAdded = true;
            listener.ListenToChanged = true;
            listener.ListenToRemoved = true;

            var a = _contextA.CreateEntity();
            var b = _contextA.CreateEntity();

            a.AddComponent(a.CreateComponent(typeof(ComponentA)));
            b.AddComponent(b.CreateComponent(typeof(ComponentA)));

            _contextA.DetachEntity(b);
            var c = _contextA.CreateEntity(b.id);
            c.AddComponent(c.CreateComponent(typeof(ComponentC)));

            // b not in context, c in context, both have same id
            Assert.NotSame(b, c);
            Assert.Equal(b.id, c.id);

            // only pull in A since B is detached and C doesn't have the right component
            var firstOnlyA = listener.Gather();
            Assert.Single(firstOnlyA);
            var emptyGet = listener.Gather();
            Assert.Empty(emptyGet);

            var aComponent = (ComponentA)a.GetComponent(typeof(ComponentA));
            var bComponent = (ComponentA)b.GetComponent(typeof(ComponentA));
            var cComponent = (ComponentC)c.GetComponent(typeof(ComponentC));
            aComponent.Value = "New Value For A";
            bComponent.Value = "New Value For B";
            cComponent.FloatyFloat = 1234.5f;

            var actualChanged = listener.Gather();
            Assert.Single(actualChanged);
            emptyGet = listener.Gather();
            Assert.Empty(emptyGet);

            a.RemoveComponent(aComponent);
            b.RemoveComponent(bComponent);
            c.RemoveComponent(cComponent);

            var actualRemoved = listener.Gather();
            Assert.Single(actualRemoved);
            emptyGet = listener.Gather();
            Assert.Empty(emptyGet);
        }
    }
}
