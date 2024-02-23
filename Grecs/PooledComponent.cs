using System.Collections.Generic;

namespace Grecs
{
    public class PooledComponent<T> : Component where T : IComponent, new()
    {
        private static Queue<object> _pool = new Queue<object>();

        public static void FlushInstances()
        {
            _pool.Clear();
        }

        public static T GetInstance()
        {
            if (_pool.Count == 0)
            {
                return new T();
            }

            return (T)_pool.Dequeue();
        }

        public override void Add(Entity owner) {
            base.Add(owner);
        }

        public override void Remove() {
            base.Remove();
            _pool.Enqueue(this);
        }

    }

}
