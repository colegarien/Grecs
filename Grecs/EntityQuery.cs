using System;
using System.Collections.Generic;

namespace Grecs
{
    public class EntityQuery
    {
        public List<Type> ands = new List<Type>();
        public List<Type> ors = new List<Type>();
        public List<Type> nones = new List<Type>();

        public bool IsEmpty()
        {
            return ands.Count == 0
                && ors.Count == 0;
        }

        public bool Matches(Entity entity)
        {
            if (IsEmpty())
                return false;

            var hasAllAnds = true;
            foreach (var type in ands)
                hasAllAnds = hasAllAnds && entity.HasComponent(type);

            var hasAnyOr = ors.Count == 0;
            foreach (var type in ors)
                hasAnyOr = hasAnyOr || entity.HasComponent(type);

            var hasNoNones = true;
            foreach(var type in nones)
                hasNoNones = hasNoNones && !entity.HasComponent(type);

            return hasAllAnds && hasAnyOr && hasNoNones;
        }

        public bool Uses(Type componentType)
        {
            return !IsEmpty() && (
                ands.Contains(componentType) ||
                ors.Contains(componentType) ||
                nones.Contains(componentType)
            );
        }

        /// <summary>
        /// Add criteria that all entities must meet
        /// </summary>
        /// <param name="types">Component Types to add to filter</param>
        /// <returns>this to allow for chaining</returns>
        public EntityQuery And(params Type[] types)
        {
            ands.AddRange(types);
            return this;
        }

        /// <summary>
        /// Add criteria that all entities must have at-least one of
        /// </summary>
        /// <param name="types">Component Types to add to filter</param>
        /// <returns>this to allow for chaining</returns>
        public EntityQuery Or(params Type[] types)
        {
            ors.AddRange(types);
            return this;
        }

        /// <summary>
        /// Filter out entities with specific components
        /// Must be used with AND/OR query
        /// </summary>
        /// <param name="types">Component Types to add to filter</param>
        /// <returns>this to allow for chaining</returns>
        public EntityQuery None(params Type[] types)
        {
            nones.AddRange(types);
            return this;
        }
    }
}
