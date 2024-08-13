using Grecs;

namespace Grecs.Test.Stub
{
    internal class ComponentA : Component
    {
        private string _value = "";

        public string Value { get => _value; set
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
