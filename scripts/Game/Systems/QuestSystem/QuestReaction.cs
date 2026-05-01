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

        public async Task Execute()
        {
            var target = ((SceneTree)Engine.GetMainLoop()).Root.GetNode(_targetPath);

            new Callable(target, _methodName).Call([.._params]);

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
    }
}
