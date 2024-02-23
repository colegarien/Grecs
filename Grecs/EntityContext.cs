using System;
using System.Collections.Generic;
using System.Linq;

namespace Grecs
{
    public class EntityContext
    {
        protected static Queue<Entity> _entityPool = new Queue<Entity>();

        protected IDictionary<uint, Entity> entities = new Dictionary<uint, Entity>();
        protected IDictionary<Type, List<uint>> entityComponents = new Dictionary<Type, List<uint>>();
        protected List<uint> recentlyRemoved = new List<uint>();

        protected List<QueryListener> _queryListeners = new List<QueryListener>();

        protected readonly ComponentEvent OnComponentAdded;
        protected readonly ComponentEvent OnComponentChanged;
        protected readonly ComponentEvent OnComponentRemoved;

        public EntityContext()
        {
            OnComponentAdded = HandleOnComponentAdded;
            OnComponentChanged = HandleOnComponentChanged;
            OnComponentRemoved = HandleOnComponentRemoved;
        }

        public Entity GetEntity(uint id)
        {
            if (!entities.ContainsKey(id)) {
                return null;
            }

            return entities[id];
        }

        public Entity[] GetEntities()
        {
            return entities.Values.ToArray();
        }

        public Entity[] GetEntities(EntityQuery query)
        {
            if (query.IsEmpty())
                return Array.Empty<Entity>();

            bool applyAnd = query.ands.Count > 0;
            var andEntityIds = entities.Keys.ToList();
            foreach (var andType in query.ands)
            {
                var entityIdsWithType = entityComponents.ContainsKey(andType)
                        ? entityComponents[andType]
                        : new List<uint>();
                andEntityIds = andEntityIds.Intersect(entityIdsWithType).ToList();

                if (andEntityIds.Count == 0)
                    break;
            }

            bool applyOr = query.ors.Count > 0;
            var orEntityIds = new List<uint>();
            foreach (var orType in query.ors)
            {
                if (entityComponents.ContainsKey(orType))
                    orEntityIds.AddRange(entityComponents[orType]);
            }

            bool applyNone = query.nones.Count > 0;
            var noneEntityIds = new List<uint>();
            foreach(var noneType in query.nones)
            {
                if (entityComponents.ContainsKey(noneType))
                    noneEntityIds.AddRange(entityComponents[noneType]);
            }

            return entities
                .Where(kv => (!applyAnd || andEntityIds.Contains(kv.Key)) &&
                    (!applyOr || orEntityIds.Contains(kv.Key)) &&
                    (!applyNone || !noneEntityIds.Contains(kv.Key))
                ).Select(kv => kv.Value)
                .ToArray();
        }

        public Entity CreateEntity(uint? id = null)
        {
            uint newId = id == null
                ? (entities.Keys.Count > 0 ? entities.Keys.Max() : 0) + 1
                : (uint)id;

            if(entities.ContainsKey(newId))
                return entities[newId];

            if (_entityPool.Count > 0)
                entities[newId] = _entityPool.Dequeue();
            else
                entities[newId] = new Entity();
            entities[newId].id = newId;
            entities[newId].OnComponentAdded += OnComponentAdded;
            entities[newId].OnComponentChanged += OnComponentChanged;
            entities[newId].OnComponentRemoved += OnComponentRemoved;

            return entities[newId];
        }

        public void DestroyEntity(uint id)
        {
            if (entities.ContainsKey(id))
            {
                recentlyRemoved.Add(id);
                entities[id].RemoveAllComponents();
                _entityPool.Enqueue(entities[id]);
                entities.Remove(id);
            }
        }

        public void DestroyEntity(Entity entity)
        {
            recentlyRemoved.Add(entity.id);

            entity.RemoveAllComponents();
            _entityPool.Enqueue(entity);
            entities.Remove(entity.id);
        }

        public void AttachEntity(Entity entity)
        {
            if (!entities.ContainsKey(entity.id) || entities[entity.id] != entity)
            {
                entity.id = (entities.Keys.Count > 0 ? entities.Keys.Max() : 0) + 1;
                entity.OnComponentAdded += OnComponentAdded;
                entity.OnComponentChanged += OnComponentChanged;
                entity.OnComponentRemoved += OnComponentRemoved;

                entities[entity.id] = entity;

                foreach (var component in entity.GetComponents()) {
                    HandleOnComponentAdded(entity, component);
                }
            }
        }

        public void DetachEntity(Entity entity)
        {
            if (entities.ContainsKey(entity.id) && entities[entity.id] == entity)
            {
                entity.OnComponentAdded -= OnComponentAdded;
                entity.OnComponentChanged -= OnComponentChanged;
                entity.OnComponentRemoved -= OnComponentRemoved;

                entities.Remove(entity.id);
                foreach (var component in entity.GetComponents())
                {
                    if (entityComponents.ContainsKey(component.GetType()))
                    {
                        entityComponents[component.GetType()].Remove(entity.id);
                    }
                }

                foreach (var listener in _queryListeners)
                    listener.Detach(entity);
            }
        }

        public uint[] GetRecentlyDestroyed()
        {
            var ids = recentlyRemoved.ToArray();
            recentlyRemoved.Clear();
            return ids;
        }

        public QueryListener CreateListener(EntityQuery query)
        {
            var listener = new QueryListener(query);
            _queryListeners.Add(listener);
            return listener;
        }

        public void HandleOnComponentAdded(Entity entity, IComponent component)
        {
            if(!entityComponents.ContainsKey(component.GetType()))
                entityComponents[component.GetType()] = new List<uint>();
            entityComponents[component.GetType()].Add(entity.id);

            UpdateListeners(QueryEventType.Added, entity, component);
        }

        public void HandleOnComponentChanged(Entity entity, IComponent component)
        {
            UpdateListeners(QueryEventType.Changed, entity, component);
        }

        public void HandleOnComponentRemoved(Entity entity, IComponent component)
        {
            if(entityComponents.ContainsKey(component.GetType()))
                entityComponents[component.GetType()].Remove(entity.id);

            UpdateListeners(QueryEventType.Removed, entity, component);
        }

        protected void UpdateListeners(QueryEventType type, Entity entity, IComponent component)
        {
            foreach (var listener in _queryListeners)
                listener.HandleEntityChangeEvent(type, entity, component);
        }
    }
}
