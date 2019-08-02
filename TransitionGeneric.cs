using System;
using System.Linq;

namespace DMJ.Standard.StateMachine.State
{
    internal class Transition<TState, TKey> : Transition where TState : BaseState<TKey> where TKey : IEquatable<TKey>
    {
        private readonly object _message;
        private readonly Func<bool>[] _condition;

        public Transition(object message, params Func<bool>[] condition)
            : base(typeof(TState))
        {
            _message = message;
            _condition = condition;
        }

        public override bool IsValid(object message) =>
            message.ToString() == _message.ToString()
                &&
            _condition.All(condition => condition());

        public override string ToString() =>
            $"Transition to {typeof(TState).Name} on {_message}";
    }
}
