using System;
using System.Collections.Generic;
using System.Linq;

namespace Grecs
{
    public delegate void ComponentEvent(Entity entity, IComponent component);

    public class Entity
    {
        protected IDictionary<Type, IComponent> _components = new Dictionary<Type, IComponent>();

        public event ComponentEvent OnComponentAdded;
        public event ComponentEvent OnComponentChanged;
        public event ComponentEvent OnComponentRemoved;

        public uint id;

        public void AddComponent(IComponent component)
        {
            _components[component.GetType()] = component;
            component.Add(this);

            if (OnComponentAdded != null)
                OnComponentAdded(this, component);
        }

        public Entity RemoveComponent(IComponent component)
        {
            var type = component.GetType();
            if (HasComponent(type))
            {
                _components.Remove(type);
                component.Remove();
            }

            TriggerComponentRemoved(component);
            return this;
        }

        public void RemoveAllComponents()
        {
            var c = _components.Values;
            foreach(var component in c)
                RemoveComponent(component);
        }

        public IComponent[] GetComponents()
        {
            return _components.Values.ToArray();
        }

        public bool HasComponent(Type type)
        {
            return _components.ContainsKey(type);
        }

        public IComponent GetComponent(Type type)
        {
            return HasComponent(type)
                ? _components[type]
                : null;
        }

        public T GetComponent<T>() where T: class
        {
            var type = typeof(T);
            return HasComponent(type)
                ? (T)_components[type]
                : null;
        }

        public IComponent CreateComponent<T>() where T : IComponent, new()
        {
            IComponent c;
            var type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PooledComponent<>))
            {
                c = PooledComponent<T>.GetInstance();
            }
            else
            {
                c = new T();
            }

            c.Add(this);
            return c;
        }

        public IComponent CreateComponent(Type type)
        {
            IComponent c;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PooledComponent<>))
            {
                c = (IComponent)type.GetMethod("GetInstance").Invoke(null,null);
            }
            else {
                c = (IComponent)Activator.CreateInstance(type);
            }

            c.Add(this);
            return c;
        }

        public void TriggerComponentAdded(IComponent component)
        {
            OnComponentAdded?.Invoke(this, component);
        }
        public void TriggerComponentChanged(IComponent component)
        {
            OnComponentChanged?.Invoke(this, component);
        }
        public void TriggerComponentRemoved(IComponent component)
        {
            OnComponentRemoved?.Invoke(this, component);
        }
    }
}
