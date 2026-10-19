using Godot;

namespace TnT.EduGame.Question
{
    // Describes how one formula param gets its value when a round is rolled.
    // Precedence: fixed value → pool → [Min, Max] range.
    [GlobalClass, Tool]
    public partial class ParamSpec : Resource
    {
        [Export] public string Name { get; set; } = "";
        [Export] public int Min { get; set; } = 1;
        [Export] public int Max { get; set; } = 10;
        // when non-empty, values are drawn from this pool instead of the range;
        // input widgets also offer exactly these values as options
        [Export] public int[] Pool { get; set; } = [];
        [Export] public bool IsFixed { get; set; }
        [Export] public int FixedValue { get; set; }

        public ParamSpec() { }
        public ParamSpec(string name) => Name = name;
    }
}
