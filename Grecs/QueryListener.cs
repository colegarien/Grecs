using System.Collections.Generic;
using System.Linq;

namespace Grecs
{
    public enum QueryEventType
    {
        Added,
        Changed,
        Removed
    }

    public class QueryListener
    {
        public bool ListenToAdded { get; set; } = true;
        public bool ListenToChanged { get; set; } = false;
        public bool ListenToRemoved { get; set; } = false;

        protected EntityQuery _query;
        protected Dictionary<uint, Entity> _entities;


        public QueryListener(EntityQuery query)
        {
            _query = query;
            _entities = new Dictionary<uint, Entity>();
        }

        public void Detach(Entity entity)
        {
            _entities.Remove(entity.id);
        }

        public Entity[] Gather()
        {
            var result = _entities.Values.ToArray();
            _entities.Clear();

            return result;
        }

        public void HandleEntityChangeEvent(QueryEventType type, Entity entity, IComponent component)
        {
            // Check if component change is even relevant
            if (!_query.Uses(component.GetType())
                || (type == QueryEventType.Added && !ListenToAdded)
                || (type == QueryEventType.Changed && !ListenToChanged))
            {
                return;
            }

            if (_query.Matches(entity))
            {
                if (!_entities.ContainsKey(entity.id))
                    _entities[entity.id] = entity;
            }
            else if (type == QueryEventType.Removed && ListenToRemoved && !_entities.ContainsKey(entity.id))
            {
                _entities[entity.id] = entity;
            }
            else if (!ListenToRemoved && _entities.ContainsKey(entity.id))
            {
                _entities.Remove(entity.id);
            }
        }
    }
}
