using System;
using System.Collections.Generic;
using System.Linq;

namespace DMJ.Standard.StateMachine.State
{
    public abstract class BaseState<TKey> where TKey : IEquatable<TKey>
    {
        public abstract TKey ID { get; }

        internal readonly SM<TKey> stateMachine;
        private readonly List<Transition> transitions = new List<Transition>();
        protected readonly List<Action> onAwake = new List<Action>();
        protected readonly List<Action> onEntry = new List<Action>();
        protected readonly List<Action> onExit = new List<Action>();

        public BaseState(SM<TKey> stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        protected TStateMachine StateMachine<TStateMachine>() where TStateMachine : SM<TKey> =>
            stateMachine as TStateMachine;

        internal virtual void Enter() =>
            onEntry.ForEach(action => action());

        internal virtual void Exit() =>
            onExit.ForEach(action => action());

        internal virtual void Awake() =>
            onAwake.ForEach(action => action());

        internal void Initialize() =>
            Awake();

        protected void OnAwake(params Action[] actions) =>
            actions.ToList().ForEach(action => onAwake.Add(action));

        protected void OnEntry(params Action[] actions) =>
            actions.ToList().ForEach(action => onEntry.Add(action));

        protected void OnExit(params Action[] actions) =>
            actions.ToList().ForEach(action => onExit.Add(action));

        internal virtual BaseState<TKey> Receive(object message)
        {
            IEnumerable<BaseState<TKey>> NewStates = transitions.Where(transition => transition.IsValid(message))
                                                             .Select(transition => (BaseState<TKey>)transition.State
                                                                                                           .GetConstructor(new Type[] { stateMachine.GetType() })
                                                                                                           .Invoke(new [] { stateMachine }))
                                                             .Take(1);
            foreach (BaseState<TKey> newState in NewStates)
            {
                StateChanging(newState, message);
                stateMachine.AddExitAction(Exit);
                stateMachine.AddAwakeAction(newState.Awake);
                stateMachine.AddEntryAction(newState.Enter);
                return newState;
            }
            return this;
        }

        private void StateChanging(BaseState<TKey> newState, object message) =>
            stateMachine.Save(this, newState, message);
        
        protected virtual Action Broadcast(object message) =>
            () => stateMachine._broadcast(message);

        protected Action Send(object message) =>
            () => stateMachine._send(message);

        protected void AddTransition<TDestination>(object message, params Func<bool>[] conditions) where TDestination : BaseState<TKey> =>
            AddATransition<TDestination>(new Transition<TDestination, TKey>(message, conditions));

        private void AddATransition<TDestination>(Transition transition) where TDestination : BaseState<TKey> =>
            transitions.Add(transition);

        public override string ToString() =>
            $"{GetType().Name} ({GetType().BaseType.Name} {ID})";
    }
}
