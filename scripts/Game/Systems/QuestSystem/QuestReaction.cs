using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using TnT.Systems;

namespace TnT.EduGame.QuestSystem
{
    [Tool]
    [GlobalClass]
    public partial class QuestReaction : Resource
    {
        [Export] public NodePath _targetPath;
        [Export] public string _methodName;
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
                GD.PrintErr("QuestReaction: Assign a target path before picking a method.");
                return;
            }

            var targetType = GodotReflectionUtils.GetCsharpType(target);
            if (targetType == null)
            {
                GD.PrintErr($"QuestReaction: Could not resolve C# type for '{target.Name}'.");
                return;
            }

            var methods = GodotReflectionUtils.GetUserMethods(targetType).ToList();
            CreatePickerWindow($"Select method on {target.Name}", methods, selectedName =>
            {
                _methodName = selectedName;
                NotifyPropertyListChanged();
            });
        }

        // Builds and shows a modal tree picker. Calls onSelected with the chosen method name.
        private static void CreatePickerWindow(string title, List<MethodInfo> methods, Action<string> onSelected)
        {
            var picker = new Window { Title = title, Size = new Vector2I(350, 500) };
            var vbox = new VBoxContainer();
            vbox.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.KeepSize, 4);

            var tree = new Tree { SizeFlagsVertical = Control.SizeFlags.ExpandFill, HideRoot = true };
            var root = tree.CreateItem();
            foreach (var method in methods)
                tree.CreateItem(root).SetText(0, method.Name);

            tree.ItemSelected += () =>
            {
                onSelected?.Invoke(tree.GetSelected()?.GetText(0));
                picker.QueueFree();
            };

            picker.CloseRequested += picker.QueueFree;
            picker.AddChild(vbox);
            vbox.AddChild(tree);

            EditorInterface.Singleton.GetBaseControl().AddChild(picker);
            picker.PopupCentered();
        }

        // Entry point. Validates target and method, then maps params and invokes.
        public async Task Execute(Node context)
        {
            var target = context.GetNodeOrNull(_targetPath);
            if (!IsInstanceValid(target) || target is not IQuestReactionObject completionObject)
            {
                GD.PrintErr($"QuestReaction: Target at '{_targetPath}' is invalid or missing IQuestReactionObject.");
                return;
            }

            // Callable.Call() doesn't dispatch params[] methods — reflection used instead
            var method = target.GetType().GetMethod(_methodName, BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
            {
                GD.PrintErr($"QuestReaction: Method '{_methodName}' not found on '{target.Name}'.");
                return;
            }

            var parameters = method.GetParameters();
            bool hasParamsArray = parameters.LastOrDefault()?.IsDefined(typeof(ParamArrayAttribute), false) ?? false;
            if (!hasParamsArray && _params.Count != parameters.Length)
            {
                GD.PrintErr($"QuestReaction: Config error — '{_methodName}' expects {parameters.Length} args, got {_params.Count}.");
                return;
            }

            try
            {
                var args = GodotReflectionUtils.MapVariantsToParameters(context, _params, parameters);
                await InvokeAndAwait(method, target, args, completionObject);
            }
            catch (Exception e)
            {
                var msg = e is TargetInvocationException ? e.InnerException?.Message : e.Message;
                GD.PrintErr($"QuestReaction: Runtime error in '{_methodName}': {msg}");
            }
        }

        // Invokes the method and waits for the observer to signal ReactionCompleted.
        // Subscribes before invoking — the method may fire the event synchronously on
        // the same frame, before a post-invoke subscribe would ever run.
        // On failure, cleans up the subscription and re-throws for Execute to log.
        private static async Task InvokeAndAwait(MethodInfo method, Node target, object[] args, IQuestReactionObject observer)
        {
            var tcs = new TaskCompletionSource();
            observer.ReactionCompleted += OnCompleted;

            void OnCompleted()
            {
                observer.ReactionCompleted -= OnCompleted;
                tcs.TrySetResult();
            }

            try
            {
                method.Invoke(target, args);
                await tcs.Task;
            }
            catch
            {
                OnCompleted();
                throw;
            }
        }
    }
}
