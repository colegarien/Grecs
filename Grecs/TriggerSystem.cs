using System.Linq;

namespace Grecs
{
    public abstract class TriggerSystem
    {
        protected abstract QueryListener GetListener(EntityContext context);
        protected abstract bool Filter(Entity entity);
        public abstract void Execute(EntityContext context, Entity[] entities);

        protected QueryListener _listener;
        
        public TriggerSystem(EntityContext context)
        {
            _listener = GetListener(context);
        }

        public void Execute(EntityContext context)
        {
            var entities = _listener.Gather()
                .Where(e => Filter(e))
                .ToArray();

            if (entities.Length > 0)
                Execute(context, entities);
        }
    }
}
