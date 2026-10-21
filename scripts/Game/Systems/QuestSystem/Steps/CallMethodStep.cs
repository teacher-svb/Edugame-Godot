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
            var method = _target?.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == _method)
                ?? throw new InvalidOperationException($"Method '{_method}' not found on '{_target?.Name}'.");

            var parameters = method.GetParameters();
            bool hasParamsArray = parameters.LastOrDefault()?.IsDefined(typeof(ParamArrayAttribute), false) ?? false;
            if (!hasParamsArray && _args.Count != parameters.Length)
                throw new InvalidOperationException($"'{_method}' expects {parameters.Length} args, got {_args.Count}.");

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

        public override string[] _GetConfigurationWarnings()
        {
            if (_target == null) return ["No target assigned."];
            if (string.IsNullOrEmpty(_method)) return ["No method selected."];
            return [];
        }
    }
}
