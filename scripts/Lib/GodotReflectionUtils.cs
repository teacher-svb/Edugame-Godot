using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace TnT.Systems
{
    // Utilities for C# reflection in Godot: method dispatch with Variant parameters,
    // and type/method discovery from node scripts.
    public static class GodotReflectionUtils
    {
        // Maps each Variant to the C# type the corresponding parameter expects.
        // NodePath resolution is type-driven: resolved to a Node only when the parameter expects one.
        // Packs trailing args into a typed array for params[] methods.
        public static object[] MapVariantsToParameters(Node context, IEnumerable<Variant> rawParams, ParameterInfo[] parameters)
        {
            var raw = rawParams.ToArray();
            var args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].IsDefined(typeof(ParamArrayAttribute), false))
                {
                    args[i] = CreateParamsArray(raw.Skip(i), parameters[i].ParameterType.GetElementType(), context);
                    break;
                }
                args[i] = ConvertVariant(raw[i], parameters[i].ParameterType, context);
            }
            return args;
        }

        // Converts a Godot Variant to the C# type required by a method parameter.
        // NodePath variants are resolved to a Node when the target type is a GodotObject subclass,
        // or kept as NodePath when the target type is NodePath.
        // GodotObject subclasses (non-path) are handled via As<GodotObject>() to preserve the runtime type.
        public static object ConvertVariant(Variant v, Type t, Node context = null)
        {
            if (v.VariantType == Variant.Type.NodePath)
            {
                if (t == typeof(NodePath)) return v.AsNodePath();
                if (typeof(GodotObject).IsAssignableFrom(t)) return context?.GetNodeOrNull(v.AsNodePath());
            }
            if (typeof(GodotObject).IsAssignableFrom(t)) return v.As<GodotObject>();
            return v.VariantType switch
            {
                Variant.Type.Bool   => v.As<bool>(),
                Variant.Type.Int    => Convert.ChangeType(v.As<long>(), t),
                Variant.Type.Float  => Convert.ChangeType(v.As<double>(), t),
                Variant.Type.String => v.As<string>(),
                _                   => v.Obj
            };
        }

        // Resolves the C# type for a node by matching the script filename to a loaded assembly type.
        public static Type GetCsharpType(Node target)
        {
            var className = target.GetScript().As<Resource>()?.ResourcePath?.GetFile().GetBaseName();
            if (className == null) return null;
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return []; } })
                .FirstOrDefault(t => t.Name == className);
        }

        // Returns public instance methods declared in the same assembly as the target type, sorted by name.
        public static IEnumerable<MethodInfo> GetUserMethods(Type targetType)
        {
            return targetType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.DeclaringType?.Assembly == targetType.Assembly)
                .OrderBy(m => m.Name);
        }

        // Packs a sequence of Variants into a strongly-typed array for params[] dispatch.
        private static Array CreateParamsArray(IEnumerable<Variant> items, Type elementType, Node context)
        {
            var arr = items.ToArray();
            var typedArr = Array.CreateInstance(elementType, arr.Length);
            for (int i = 0; i < arr.Length; i++)
                typedArr.SetValue(ConvertVariant(arr[i], elementType, context), i);
            return typedArr;
        }
    }
}
