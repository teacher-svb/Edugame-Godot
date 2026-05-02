using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;

namespace TnT.EduGame.QuestSystem
{
    [Tool]
    [GlobalClass]
    public partial class QuestReaction : Resource
    {
        [Export] NodePath _targetPath;
        [Export] string _methodName;
        [Export] Godot.Collections.Array<Variant> _params = [];

        [ExportToolButton("Pick Method")]
        public Callable PickMethodButton => Callable.From(OpenMethodPicker);

        public override void _ValidateProperty(Godot.Collections.Dictionary property)
        {
            if (property["name"].AsStringName() == PropertyName._methodName)
                property["usage"] = (int)(property["usage"].As<PropertyUsageFlags>() | PropertyUsageFlags.ReadOnly);
        }

        private Node ResolveEditorTarget()
        {
            var sceneRoot = EditorInterface.Singleton.GetEditedSceneRoot();
            if (sceneRoot == null) return null;

            foreach (var node in sceneRoot.FindChildren("*", "", true, false))
            {
                var prop = node.Get("_reactions");
                if (prop.VariantType != Variant.Type.Array) continue;

                var reactions = prop.As<Godot.Collections.Array<QuestReaction>>();
                if (reactions?.Contains(this) == true)
                    return node.GetNodeOrNull(_targetPath);
            }
            return null;
        }

        private void OpenMethodPicker()
        {
            var target = ResolveEditorTarget();

            if (target == null)
            {
                GD.PrintErr("QuestReaction: assign a target path before picking a method.");
                return;
            }

            var picker = new Window
            {
                Title = $"Select method on {target.Name}",
                Size = new Vector2I(350, 500)
            };

            var vbox = new VBoxContainer();
            vbox.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.KeepSize, 4);
            picker.AddChild(vbox);

            var tree = new Tree
            {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                HideRoot = true
            };
            vbox.AddChild(tree);

            var root = tree.CreateItem();
            var gameAssembly = typeof(QuestReaction).Assembly;
            var scriptPath = target.GetScript().As<Resource>()?.ResourcePath;
            var className = scriptPath?.GetFile().GetBaseName();
            var targetType = className != null
                ? gameAssembly.GetTypes().FirstOrDefault(t => t.Name == className)
                : null;

            if (targetType == null)
            {
                GD.PrintErr($"QuestReaction: could not resolve C# type from script '{scriptPath}'.");
                picker.QueueFree();
                return;
            }

            var methods = targetType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.DeclaringType?.Assembly == gameAssembly)
                .OrderBy(m => m.Name);

            foreach (var method in methods)
            {
                var item = tree.CreateItem(root);
                item.SetText(0, method.Name);
            }

            tree.ItemSelected += () =>
            {
                var selected = tree.GetSelected();
                if (selected == null) return;
                _methodName = selected.GetText(0);
                NotifyPropertyListChanged();
                picker.QueueFree();
            };

            EditorInterface.Singleton.GetBaseControl().AddChild(picker);
            picker.PopupCentered();
            picker.CloseRequested += picker.QueueFree;
        }

        public async Task Execute(Node context)
        {
            // Resolve _targetPath relative to the owning QuestEventListener
            var target = context.GetNode(_targetPath);

            // Use reflection to find the method — Callable.Call() doesn't dispatch params[] methods
            var method = target.GetType().GetMethod(_methodName, BindingFlags.Public | BindingFlags.Instance);
            if (method == null) { GD.PrintErr($"QuestReaction: method '{_methodName}' not found on '{target.Name}'."); return; }

            // NodePath variants are converted to actual Node references before passing
            var resolvedParams = _params.Select(p =>
                p.VariantType == Variant.Type.NodePath
                    ? Variant.From(context.GetNode(p.As<NodePath>()))
                    : p).ToArray();

            var parameters = method.GetParameters();
            var lastParam = parameters.LastOrDefault();
            object[] args;

            if (lastParam?.IsDefined(typeof(ParamArrayAttribute), false) == true)
            {
                // params method: convert leading regular args, pack the rest into a typed array
                var regularArgs = parameters.Take(parameters.Length - 1)
                    .Select((p, i) => ConvertVariant(resolvedParams[i], p.ParameterType)).ToArray();
                var elementType = lastParam.ParameterType.GetElementType();
                var remainder = resolvedParams.Skip(parameters.Length - 1).ToArray();
                var arr = Array.CreateInstance(elementType, remainder.Length);
                for (int i = 0; i < remainder.Length; i++)
                    arr.SetValue(ConvertVariant(remainder[i], elementType), i);
                args = [..regularArgs, arr];
            }
            else
            {
                // Regular method: convert each Variant to the expected C# parameter type
                args = [..resolvedParams.Select((p, i) => ConvertVariant(p, parameters[i].ParameterType))];
            }

            method.Invoke(target, args);

            // If the target implements IQuestReactionObject, wait for it to signal completion
            // before returning, so reactions execute sequentially in QuestEventListener
            if (target is IQuestReactionObject completionObject)
            {
                var tcs = new TaskCompletionSource();
                completionObject.ReactionCompleted += Complete;
                await tcs.Task;

                void Complete()
                {
                    completionObject.ReactionCompleted -= Complete;
                    tcs.SetResult();
                }
            }
        }

        private static object ConvertVariant(Variant v, Type t)
        {
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
    }
}
