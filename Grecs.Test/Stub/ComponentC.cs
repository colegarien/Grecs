using Grecs;

namespace Grecs.Test.Stub
{
    internal class ComponentC : Component
    {
        private float floatyFloat;
        public float FloatyFloat {
            get => floatyFloat;
            set {
                if (floatyFloat != value)
                {
                    floatyFloat = value;
                    TriggerChange();
                }
            }
        }
    }
}
