using System.Collections.Generic;

namespace Grecs
{
    public class SystemCoordinator
    {
        protected List<TickSystem> tickSystems = new List<TickSystem>();
        protected List<TriggerSystem> triggerSystems = new List<TriggerSystem>();

        public SystemCoordinator Add(TickSystem system)
        {
            tickSystems.Add(system);
            return this;
        }

        public SystemCoordinator Add(TriggerSystem system)
        {
            triggerSystems.Add(system);
            return this;
        }

        public void Execute(float deltaTime, EntityContext context)
        {
            foreach(var system in tickSystems)
            {
                system.Execute(deltaTime, context);
            }

            foreach (var system in triggerSystems)
            {
                system.Execute(context);
            }
        }
    }
}
