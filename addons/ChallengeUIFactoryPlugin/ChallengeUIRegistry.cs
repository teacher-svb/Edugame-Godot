using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using TnT.Systems.UI;
using Godot;

public static class ChallengeUIRegistry
{
    private static Dictionary<string, Type> _typesByName = new();
    private static Dictionary<Type, IChallengeUIStrategy> _strategies = new();

    public static void Initialize()
    {
        var strategyType = typeof(IChallengeUIStrategy);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null); }
            })
            .Where(t => t != null && !t.IsAbstract && strategyType.IsAssignableFrom(t));

        _typesByName = types.ToDictionary(t => t.Name, t => t);
        _strategies = types.ToDictionary(t => t, t => Activator.CreateInstance(t) as IChallengeUIStrategy);
    }

    public static string[] GetNames()
    {
        return _typesByName.Keys.ToArray();
    }

    static bool TryGetType(string name, out Type type) =>
        _typesByName.TryGetValue(name, out type);
    public static bool TryGetStrategy(string name, out IChallengeUIStrategy strategy)
    {
        TryGetType(name, out var t);
        return _strategies.TryGetValue(t, out strategy);
    }
}
