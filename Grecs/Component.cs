namespace Grecs
{
    public class Component : IComponent
    {
        public Entity Owner { get; set; }
        public virtual void Add(Entity owner) { Owner = owner; }
        public virtual void Remove(){ Owner = null; }

        protected void TriggerChange() { Owner?.TriggerComponentChanged(this); }
    }
}
