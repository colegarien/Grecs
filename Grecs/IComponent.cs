namespace Grecs
{
    public interface IComponent
    {
        public Entity Owner { get; set; }
        public void Add(Entity owner);
        public void Remove();
    }
}
