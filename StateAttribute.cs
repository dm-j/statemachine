using System;

namespace DMJ.Standard.StateMachine.State
{
    /// <summary>
    /// Apply this to a State so that you can find and persist it with a StateLookup object
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class StateAttribute : Attribute
    {
        public object ID { get; private set; }
        public object StateType { get; private set; }

        public StateAttribute(object ID, object StateType)
        {
            this.ID = ID;
            this.StateType = StateType;
        }
    }
}
