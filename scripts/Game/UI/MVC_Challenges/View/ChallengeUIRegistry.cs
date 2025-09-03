using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using TnT.Systems.UI;

public static class ChallengeUIRegistry
{
    private static Dictionary<string, Type> _typesByName;

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
    }

    public static string[] GetNames() => _typesByName.Keys.ToArray();

    public static bool TryGetType(string name, out Type type) =>
        _typesByName.TryGetValue(name, out type);
}
