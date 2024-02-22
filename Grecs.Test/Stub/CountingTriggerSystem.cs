using Grecs;
using System;

namespace Grecs.Test.Stub
{
    internal class CountingTriggerSystem : TriggerSystem
    {
        public static int Count = 0;
        public int InstanceCount = -1;
        public int TriggerCount = 0;

        public CountingTriggerSystem(EntityContext context) : base(context)
        {
        }

        protected override QueryListener GetListener(EntityContext context)
        {
            var query = new EntityQuery().And(typeof(ComponentA));
            return context.CreateListener(query);
        }

        protected override bool Filter(Entity entity)
        {
            return true;
        }

        public override void Execute(EntityContext context, Entity[] entities)
        {
            Count++;
            InstanceCount = Count;
            TriggerCount++;
        }
    }
}
