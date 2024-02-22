using Grecs;
using System.Linq;

namespace Grecs.Test.Stub
{
    internal class AddRemoveEntityTickSystem : TickSystem
    {
        public override void Execute(float deltaTime, EntityContext context)
        {
            var query = new EntityQuery().Or(typeof(ComponentA));

            var entity = context.GetEntities(query).FirstOrDefault();
            if (entity != null)
            {
                context.DestroyEntity(entity);

                var entityB = context.CreateEntity();
                entityB.AddComponent(entityB.CreateComponent(typeof(ComponentB)));
            }
        }
    }
}
