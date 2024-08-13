namespace Grecs.Test.Stub
{
    internal class PooledComponentB: PooledComponent<PooledComponentB>
    {
        private int _someNumber;
        public int SomeNumber
        {
            get => _someNumber; set
            {
                if (_someNumber != value)
                {
                    _someNumber = value;
                    TriggerChange();
                }
            }
        }
    }
}
