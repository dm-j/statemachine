using System;

namespace DMJ.Standard.StateMachine.State
{
    public abstract class RootStateMachine<TKey> : SM<TKey> where TKey : IEquatable<TKey>
    {
        private readonly ISaveState<TKey> save;
        private readonly string user;

        public RootStateMachine(TKey id, ISaveState<TKey> save, string user)
            : base(id)
        {
            this.save = save;
            this.user = user;
        }

        protected override void Broadcast(object message) =>
            _sendDownwards(message);

        protected override void _save(SM<TKey> record, BaseState<TKey> oldState, BaseState<TKey> newState, object message) =>
            save.Save(record, oldState, newState, user, message);
    }
}
