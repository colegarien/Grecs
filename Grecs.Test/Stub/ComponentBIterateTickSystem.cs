using Grecs;

namespace Grecs.Test.Stub
{
    internal class ComponentBIterateTickSystem : TickSystem
    {
        public override void Execute(float deltaTime, EntityContext context)
        {
            var query = new EntityQuery();
            query.And(typeof(ComponentB));

            var bEntities = context.GetEntities(query);
            foreach(var entity in bEntities)
            {
                var c = (ComponentB)entity.GetComponent(typeof(ComponentB));
                c.SomeNumber++;
            }
        }
    }
}
