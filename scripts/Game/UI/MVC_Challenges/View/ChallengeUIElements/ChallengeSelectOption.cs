
using Godot;
using TnT.Extensions;

namespace TnT.Systems.UI
{
    public partial class ChallengeSelectOption : Button
    {
        // private static readonly CustomStyleProperty<float> s_AspectRatio = new("--aspect-ratio");
        private float aspectRatio { get; set; } = 1.0f / 1.0f;
        int _index = 0;
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                // this.RemoveFromClassList($"value-{Index}");
                _index = value;
                // this.AddClass($"value-{Index}");
            }
        }
        public string ParamName { get; set; }
        int _value;
        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                var paramtext = ParamName != "" ? $"{ParamName} = " : "";
                // Text.Text = $"{paramtext}{_value}";
            }
        }
        bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                // this.EnableInClassList("selected", value);
                _isSelected = value;
            }
        }

        // public TextElement Text { get; private set; }

        public ChallengeSelectOption(int index, string paramName, int value)
        {
            // Text = this.CreateChild<TextElement>();

            // this.Index = index;
            // this.ParamName = paramName;
            // this.Value = value;
            // IsSelected = paramName != "";


            // this.AddClass("challenge-select__option");
            // this.AddClass("button");
            // this.AddClass($"value-{index}");

            // RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            // RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        // private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        // {
        //     // If the custom style property is resolved for this element, you can query its value through the `customStyle` accessor.
        //     if (evt.customStyle.TryGetValue(s_AspectRatio, out var value))
        //         aspectRatio = value;
        //     else
        //         aspectRatio = 16f / 9f;
        // }

        // private void OnGeometryChanged(GeometryChangedEvent evt)
        // {
        //     VisualElement element = evt.target as VisualElement;

        //     // Get the resolved style of the VisualElement
        //     float width = element.resolvedStyle.width;

        //     element.style.height = width * 1f / aspectRatio;
        // }
    }
}