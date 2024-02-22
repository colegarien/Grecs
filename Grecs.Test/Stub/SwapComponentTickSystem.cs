using Grecs;

namespace Grecs.Test.Stub
{
    internal class SwapComponentTickSystem : TickSystem
    {
        public override void Execute(float deltaTime, EntityContext context)
        {
            var query = new EntityQuery().Or(typeof(ComponentA), typeof(ComponentB));

            var entities = context.GetEntities(query);

            foreach (var entity in entities)
            {
                if (entity.HasComponent(typeof(ComponentA)))
                {
                    entity.RemoveComponent(entity.GetComponent(typeof(ComponentA)));
                    entity.AddComponent(entity.CreateComponent(typeof(ComponentB)));
                }
                else if (entity.HasComponent(typeof(ComponentB)))
                {
                    entity.RemoveComponent(entity.GetComponent(typeof(ComponentB)));
                    entity.AddComponent(entity.CreateComponent(typeof(ComponentA)));
                }
            }
        }
    }
}
