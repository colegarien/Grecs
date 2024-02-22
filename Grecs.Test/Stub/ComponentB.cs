using Grecs;

namespace Grecs.Test.Stub
{
    internal class ComponentB : Component
    {
        private int _someNumber;
        public int SomeNumber
        {
            get => _someNumber; set
            {
                if (_someNumber != value)
                {
                    _someNumber = value;
                    Owner?.TriggerComponentChanged(this);
                }
            }
        }
    }
}
