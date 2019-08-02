#pragma warning disable IDE1006 // Naming Styles
using System;
using System.Collections.Generic;
using System.Linq;

namespace DMJ.Standard.StateMachine.State
{
    public abstract class SM<TKey> where TKey : IEquatable<TKey>
    {
        public TKey ID { get; set; }

        protected List<SM<TKey>> parent         = new List<SM<TKey>>();
        protected List<SM<TKey>> children       = new List<SM<TKey>>();
        protected List<BaseState<TKey>> states  = new List<BaseState<TKey>>();

        protected Queue<object> messages     = new Queue<object>();
        protected Queue<Action> entryActions = new Queue<Action>();
        protected Queue<Action> exitActions  = new Queue<Action>();
        protected Queue<Action> awakeActions = new Queue<Action>();

        internal void AddEntryAction(Action action) =>
            entryActions.Enqueue(action);

        internal void AddExitAction(Action action) =>
            exitActions.Enqueue(action);

        internal void AddAwakeAction(Action action) =>
            awakeActions.Enqueue(action);

        public SM(TKey id, SM<TKey> parent)
        {
            this.ID = id;
            this.parent.Add(parent);
        }

        protected SM(TKey id)
        {
            this.ID = id;
        }

        public SM<TKey> AddChild(SM<TKey> stateMachine)
        {
            children.Add(stateMachine);
            if (!stateMachine.parent.Contains(this))
                stateMachine.parent.Add(this);
            return this;
        }

        public SM<TKey> AddState<T>() where T : BaseState<TKey> =>
            AddState((T)typeof(T).GetConstructor(new Type[] { GetType() })
                                 .Invoke(new [] { this }));

        public SM<TKey> AddState<T>(T newState) where T : BaseState<TKey>
        {
            newState.Initialize();
            states.Add(newState);
            return this;
        }

        protected IEnumerable<T> Child<T>() where T : SM<TKey> =>
            children.OfType<T>();

        protected T Child<T>(object ID) where T : SM<TKey> =>
            Child<T>().Where(stateMachine => stateMachine.ID.Equals(ID))
                      .First();

        protected T Parent<T>() where T : SM<TKey> =>
            parent.OfType<T>().First();

        protected IEnumerable<T> State<T>() where T : BaseState<TKey> =>
            states.OfType<T>();

        protected IEnumerable<T> State<T>(TKey ID) where T : BaseState<TKey> =>
            State<T>().Where(state => state.ID.Equals(ID));

        protected virtual void Broadcast(object message) =>
            parent.ForEach(_parent => _parent.Broadcast(message));

        internal void _broadcast(object message) =>
            Broadcast(message);

        internal void _sendDownwards(object message)
        {
            Receive(message);
            children.ForEach(child => child._sendDownwards(message));
        }

        internal void _sendUpwards(object message)
        {
            Receive(message);
            parent.ForEach(_parent => _parent._sendUpwards(message));
        }

        protected virtual void Send(object message) =>
            _send(message);

        internal void _send(object message)
        {
            Receive(message);
            parent.ForEach(parent => parent._sendUpwards(message));
            children.ForEach(child => child._sendDownwards(message));
        }

        private void Receive(object message)
        {
            messages.Enqueue(message);

            while (messages.Count > 0)
                _receive(messages.Dequeue());
        }

        private void _receive(object message)
        {
            states = states.Select(state => state.Receive(message))
                           .ToList();

            while (exitActions.Count > 0)
                exitActions.Dequeue()();

            while (awakeActions.Count > 0)
                awakeActions.Dequeue()();

            while (entryActions.Count > 0)
                entryActions.Dequeue()();
        }

        public void Save(BaseState<TKey> oldState, BaseState<TKey> newState, object message) =>
            _save(this, oldState, newState, message);

        protected virtual void _save(SM<TKey> record, BaseState<TKey> oldState, BaseState<TKey> newState, object message) =>
            parent.ForEach(_parent => _parent._save(record, oldState, newState, message));

        public override string ToString() =>
            Describe(0);

        protected string Describe(int level)
        {
            string indentSpace = "  ";
            string indent = string.Join(string.Empty, Enumerable.Repeat(indentSpace, level));
            string result = $"{indent}+ {GetType().Name} {ID}";
            if (states.Any())
            {
                result += $"{Environment.NewLine}{indent}{indentSpace}{string.Join($"{Environment.NewLine}{indent}{indentSpace}", states.Select(o => o.ToString()))}";
            }
            if (children.Any())
            {
                result += $"{Environment.NewLine}{string.Join(Environment.NewLine, children.Select(o => o.Describe(level + 1)))}";
            }
            return result;
        }
    }
}
#pragma warning restore IDE1006 // Naming Styles
