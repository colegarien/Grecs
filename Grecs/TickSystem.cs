namespace Grecs
{
    public abstract class TickSystem
    {
        public abstract void Execute(float deltaTime, EntityContext context);
    }
}
