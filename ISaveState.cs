using DMJ.Standard.StateMachine.State;
using System;

namespace DMJ.Standard.StateMachine
{
    public interface ISaveState<TKey> where TKey : IEquatable<TKey>
    {
        void Save(SM<TKey> record, BaseState<TKey> oldState, BaseState<TKey> newState, string user, object message);
    }
}
