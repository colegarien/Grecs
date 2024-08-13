namespace Grecs.Demo.Components
{
    internal class NonPooledIntention: Component
    {
        private bool _isDone;
        public bool IsDone
        {
            get => _isDone; set
            {
                if (_isDone != value)
                {
                    _isDone = value;
                    TriggerChange();
                }
            }
        }
    }
}
