using System;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Godot;
using TnT.Systems;

namespace TnT.EduGame.QuestSystem
{
    /// <summary>
    /// Generic step: calls any public method on the target, for one-off reactions that don't
    /// deserve their own step type. Awaits the method if it returns a Task.
    /// </summary>
    /// <remarks>
    /// Args are mapped to the method's parameter types; a NodePath arg is resolved to a node
    /// relative to this step when the parameter expects a node. Prefer a typed step for
    /// anything taking nodes: these paths are plain values and don't follow moved nodes.
    /// </remarks>
    [Tool]
    [GlobalClass]
    public partial class CallMethodStep : QuestStep
    {
        Node _target;

        [Export]
        Node Target
        {
            get => _target;
            set
            {
                _target = value;
                NotifyPropertyListChanged(); // refresh the method dropdown
            }
        }

        [Export] string _method;
        [Export] Godot.Collections.Array _args = [];

        // Turns _method into a dropdown of the target's methods
        public override void _ValidateProperty(Godot.Collections.Dictionary property)
        {
            if (property["name"].AsStringName() != PropertyName._method || _target == null)
                return;

            var type = GodotReflectionUtils.GetCsharpType(_target);
            if (type == null)
                return;

            var names = GodotReflectionUtils.GetUserMethods(type).Select(m => m.Name).Distinct();
            property["hint"] = (int)PropertyHint.Enum;
            property["hint_string"] = string.Join(",", names);
        }

        public override async Task Run(QuestObjective objective)
        {
            var method = FindMethod();
            var parameters = method.GetParameters();
            var args = GodotReflectionUtils.MapVariantsToParameters(this, _args, parameters);
            try
            {
                if (method.Invoke(_target, args) is Task task)
                    await task;
            }
            catch (TargetInvocationException e) when (e.InnerException != null)
            {
                // rethrow the method's own exception, keeping its stack trace
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            }
        }

        // The overload that takes these args: the number of args first (a params array takes any number from its
        // position on), then the arg types; was: the first method with the name, whatever its parameters
        MethodInfo FindMethod()
        {
            var overloads = _target?.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == _method)
                .ToList();
            if (overloads == null || overloads.Count == 0)
                throw new InvalidOperationException($"Method '{_method}' not found on '{_target?.Name}'.");

            var byCount = overloads.Where(m => AcceptsCount(m.GetParameters(), _args.Count))
                // an exact count before a params array
                .OrderBy(m => HasParamsArray(m.GetParameters()) ? 1 : 0)
                .ToList();
            if (byCount.Count == 0)
                throw new InvalidOperationException(
                    $"'{_method}' on '{_target.Name}' takes {string.Join(" or ", overloads.Select(m => m.GetParameters().Length).Distinct())} args, got {_args.Count}.");

            return byCount.FirstOrDefault(m => ArgsFit(m.GetParameters())) ?? byCount[0];
        }

        static bool HasParamsArray(ParameterInfo[] parameters) =>
            parameters.LastOrDefault()?.IsDefined(typeof(ParamArrayAttribute), false) ?? false;

        static bool AcceptsCount(ParameterInfo[] parameters, int count) =>
            HasParamsArray(parameters) ? count >= parameters.Length - 1 : count == parameters.Length;

        bool ArgsFit(ParameterInfo[] parameters)
        {
            for (int i = 0; i < _args.Count; i++)
            {
                var type = i < parameters.Length && !parameters[i].IsDefined(typeof(ParamArrayAttribute), false)
                    ? parameters[i].ParameterType
                    : parameters[^1].ParameterType.GetElementType();
                if (!Fits(_args[i], type))
                    return false;
            }
            return true;
        }

        // whether GodotReflectionUtils.ConvertVariant can turn the arg into this parameter type
        static bool Fits(Variant v, Type t) => v.VariantType switch
        {
            Variant.Type.Nil      => !t.IsValueType,
            Variant.Type.Bool     => t == typeof(bool) || t == typeof(object),
            Variant.Type.Int      => IsNumber(t) || t.IsEnum || t == typeof(object),
            Variant.Type.Float    => t == typeof(float) || t == typeof(double) || t == typeof(decimal) || t == typeof(object),
            Variant.Type.String or Variant.Type.StringName =>
                t == typeof(string) || t == typeof(StringName) || t == typeof(object)
                || (t.IsEnum && Enum.TryParse(t, v.AsString(), ignoreCase: true, out _)),
            Variant.Type.NodePath => t == typeof(NodePath) || typeof(GodotObject).IsAssignableFrom(t),
            Variant.Type.Object   => typeof(GodotObject).IsAssignableFrom(t) || t == typeof(object),
            _                     => true,   // anything else is passed as is: let the call decide
        };

        static bool IsNumber(Type t) =>
            t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(byte) || t == typeof(uint)
            || t == typeof(ulong) || t == typeof(ushort) || t == typeof(sbyte) || t == typeof(float) || t == typeof(double)
            || t == typeof(decimal);

        public override string[] _GetConfigurationWarnings()
        {
            if (_target == null) return ["No target assigned."];
            if (string.IsNullOrEmpty(_method)) return ["No method selected."];
            return [];
        }
    }
}
