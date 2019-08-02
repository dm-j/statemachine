using System;

namespace DMJ.Standard.StateMachine.State
{
    internal abstract class Transition
    {
        public Transition(Type state)
        {
            State = state;
        }

        public readonly Type State;
        public abstract bool IsValid(object message);
    }
}
