using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using TnT.Systems.UI;
using Godot;

public static class ChallengeUIRegistry
{
    private static Dictionary<ChallengeUIType, Type> _types = new();
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

        _types = types.ToDictionary(t => Enum.Parse<ChallengeUIType>(t.Name), t => t);
        _strategies = types.ToDictionary(t => t, t => Activator.CreateInstance(t) as IChallengeUIStrategy);

        Validate(types);
    }

    public static ChallengeUIType[] GetRegisteredTypes()
    {
        return _types.Keys.ToArray();
    }

    private static void Validate(IEnumerable<Type> types)
    {
        var enumValues = Enum.GetValues(typeof(ChallengeUIType)).Cast<ChallengeUIType>().ToHashSet();
        var typeKeys = _types.Keys.ToHashSet();

        // 1) Enum values without matching types
        var missingTypes = enumValues.Except(typeKeys).ToList();
        if (missingTypes.Any())
        {
            throw new InvalidOperationException(
                $"ChallengeUIRegistry: Missing strategy classes for enum values: {string.Join(", ", missingTypes)}"
            );
        }

        // 2) Types without matching enum values
        var extraTypes = types
            .Where(t => !Enum.TryParse<ChallengeUIType>(t.Name, out _))
            .Select(t => t.Name)
            .ToList();

        if (extraTypes.Any())
        {
            throw new InvalidOperationException(
                $"ChallengeUIRegistry: Found strategy classes without matching enum values: {string.Join(", ", extraTypes)}"
            );
        }
    }

    static bool TryGetType(ChallengeUIType uiType, out Type type) =>
        _types.TryGetValue(uiType, out type);
    public static bool TryGetStrategy(ChallengeUIType uiType, out IChallengeUIStrategy strategy)
    {
        TryGetType(uiType, out var t);
        return _strategies.TryGetValue(t, out strategy);
    }
}
