using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;

namespace TnT.EduGame.QuestSystem
{
    [Tool]
    [GlobalClass]
    public partial class QuestReaction : Node
    {
        [Export] Node _target;
        [Export] string _methodName;
        [Export] Godot.Collections.Array<Variant> _params = [];

        [ExportToolButton("Pick Method")]
        public Callable PickMethodButton => Callable.From(OpenMethodPicker);

        [Signal] public delegate void ReactionStartEventHandler();
        [Signal] public delegate void ReactionEndEventHandler();

        private void OpenMethodPicker()
        {
            if (_target == null)
            {
                GD.PrintErr("QuestReaction: assign a target node before picking a method.");
                return;
            }

            var picker = new Window
            {
                Title = $"Select method on {_target.Name}",
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
            var scriptPath = _target.GetScript().As<Resource>()?.ResourcePath;
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
            EmitSignal(SignalName.ReactionStart);

            new Callable(_target, _methodName).Call([.._params]);

            if (_target is IQuestReactionObject completionObject)
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

            EmitSignal(SignalName.ReactionEnd);
        }
    }
}
