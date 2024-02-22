using Grecs;

namespace Grecs.Test.Stub
{
    internal class NumberIncrementTickSystem : TickSystem
    {
        public static int TickCount = 0;
        public int InstanceTick = -1;

        public override void Execute(float deltaTime, EntityContext context)
        {
            // iterate static tickCount and save value in instance variable
            TickCount++;
            InstanceTick = TickCount;
        }
    }
}
