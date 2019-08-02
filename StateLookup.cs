using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DMJ.Standard.StateMachine.State
{
    /// <summary>
    /// Finds a State by its StateAttribute ID
    /// </summary>
    public class StateLookup<TKey> where TKey : IEquatable<TKey>
    {
        private readonly Lazy<Dictionary<object, Dictionary<object, Type>>> states;

        public StateLookup(string nameSpace)
        {
            Assembly ass = Assembly.GetCallingAssembly();
            states = new Lazy<Dictionary<object, Dictionary<object, Type>>>(() => AssembleDictionary(nameSpace, ass));
        }

        public TState Get<TState>(TKey id, object type, SM<TKey> parent) where TState : BaseState<TKey>
        {
            if (states.Value.TryGetValue(type, out var result) && result.TryGetValue(id, out var value))
            {
                return value.GetConstructor(new[] { parent.GetType() })
                            .Invoke(new[] { parent })
                                as TState;
            }
            throw new KeyNotFoundException($"Cannot find State {id} of type {type}");
        }

        private Dictionary<object, Dictionary<object, Type>> AssembleDictionary(string nameSpace, Assembly assembly) =>
            assembly.GetTypes()
                    .Where(type => type.Namespace == nameSpace)
                    .Where(type => Attribute.IsDefined(type, typeof(StateAttribute)))
                    .Select(type => new { StateAttribute = type.GetCustomAttribute<StateAttribute>(), StateInstanceType = type })
                    .GroupBy(thing => thing.StateAttribute.StateType)
                    .ToDictionary(group => group.First().StateAttribute.StateType,
                                  group => group.ToDictionary(i => i.StateAttribute.ID,
                                                              i => i.StateInstanceType));
    }
}
