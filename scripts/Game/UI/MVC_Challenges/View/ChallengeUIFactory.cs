
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.EduGame.Question;

namespace TnT.Systems.UI
{
    public static class ChallengeUIFactory
    {
        // TODO: remove the whole Types thing, and replace with a simple Enum
        // [InitializeOnLoadMethod]
        static void OnProjectLoadedInEditor()
        {
            GetRegisteredTypes();

            var strats = _strategies
                .Keys
                .ToList()
                .Select(t => $"- {t}")
                .Aggregate((a, b) => $"{a}\n{b}");
            GD.Print($"Registered strategies:\n{strats}");
        }
        private static readonly Dictionary<Type, IChallengeUIStrategy> _strategies = new();

        // TODO: add second Generic type, with restriction to Enum
        public static void Register<T>() where T : IChallengeUIStrategy, new()
        {
            _strategies[typeof(T)] = new T();
        }

        public static ChallengeUI Build(IMathChallenge challenge)
        {
            if (!_strategies.TryGetValue(challenge.SelectedVisualType, out var strategy))
                throw new NotSupportedException($"No visualization strategy registered for {challenge.SelectedVisualType}");

            return strategy.Build(challenge);
        }

        // TODO: remove the whole Types thing, and replace with a simple Enum
        public static List<Type> GetRegisteredTypes()
        {
            if (_strategies.Count == 0)
                AutoRegisterAllStrategies();

            return _strategies.Keys.ToList();
        }

        // TODO: remove the whole Types thing, and replace with a simple Enum
        private static void AutoRegisterAllStrategies()
        {
            _strategies.Clear();
            var strategyType = typeof(IChallengeUIStrategy);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && strategyType.IsAssignableFrom(t));

            foreach (var type in types)
            {
                if (!_strategies.ContainsKey(type))
                {
                    if (Activator.CreateInstance(type) is IChallengeUIStrategy instance)
                        _strategies[type] = instance;
                }
            }
        }
    }
}