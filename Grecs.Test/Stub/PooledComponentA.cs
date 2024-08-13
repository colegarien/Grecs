namespace Grecs.Test.Stub
{
    internal class PooledComponentA: PooledComponent<PooledComponentA>
    {
        private string _value = "";

        public string Value
        {
            get => _value; set
            {
                if (_value != value)
                {
                    _value = value;
                    TriggerChange();
                }
            }
        }
    }
}
