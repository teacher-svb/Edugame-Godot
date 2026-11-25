using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TnT.EduGame.Challenges;

namespace TnT.EduGame.UI
{
    // A key lying on its side: bow on the left, blade to the right, one tooth per GraphBar
    // hanging from the blade, in reading order. Known teeth are cut already; the player cuts
    // the unknown ones: click or drag in a tooth's column to set its depth, mouse wheel, or
    // left/right to pick a tooth and down/up to cut deeper/shallower. It doubles as the
    // challenge's input: every unknown tooth reports its depth, from 0 at the start, so Submit
    // works as with the other inputs; the round is only checked on Submit.
    public partial class KeyWidget : ChallengeVisualizationWidget<KeyVisualization>, IChallengeInput
    {
        [Export] public Color PaperColor { get; set; } = ChallengePalette.Paper;
        [Export] public Color MetalColor { get; set; } = new(0.83f, 0.66f, 0.26f);
        [Export] public Color MetalShadeColor { get; set; } = new(0.60f, 0.45f, 0.15f);
        [Export] public Color GridColor { get; set; } = ChallengePalette.Grid;
        [Export] public Color LabelColor { get; set; } = ChallengePalette.Ink;
        [Export] public Color SlotColor { get; set; } = new(0.35f, 0.25f, 0.15f, 0.45f);
        [Export] public Color SelectedColor { get; set; } = ChallengePalette.Accent;

        public event Action<string, int> ValueAssigned;
        // a cut tooth always has a depth, so the key never takes a value back
        public event Action<string> ValueCleared { add { } remove { } }

        // the player's depth per unknown tooth param
        readonly Dictionary<string, int> _depths = new();
        List<GraphBar> _teeth = new();
        int _selected = -1;     // index in _teeth, always an unknown tooth (or -1 when none)
        bool _dragging;
        Layout _layout;

        public override void _Ready()
        {
            FocusMode = FocusModeEnum.All;
            MouseFilter = MouseFilterEnum.Stop;

            _teeth = Settings.Bars.Where(b => b != null && b.Param != "").ToList();
            foreach (var tooth in _teeth.Where(t => Round.IsUnknown(t.Param)))
                _depths[tooth.Param] = 0;
            _selected = _teeth.FindIndex(t => Round.IsUnknown(t.Param));

            Resized += QueueRedraw;
            FocusEntered += QueueRedraw;
            FocusExited += QueueRedraw;

            // ChallengeUI subscribes after adding the widget, so report the starting depths a step later
            // (focus: ChallengeUI.FocusInput once the view is shown, see FocusTarget)
            Callable.From(ReportAll).CallDeferred();
        }

        public override Control FocusTarget => this;

        // only the teeth are inputs; ChallengeUI falls back to an input widget for other unknowns
        public bool Handles(string param) => Settings.Bars.Any(b => b != null && b.Param == param);

        void ReportAll()
        {
            foreach (var (param, depth) in _depths)
                ValueAssigned?.Invoke(param, depth);
        }

        int Step => Mathf.Max(Settings.AxisStep, 1);

        void SetDepth(int tooth, int depth)
        {
            if (tooth < 0 || tooth >= _teeth.Count || !Round.IsUnknown(_teeth[tooth].Param))
                return;
            var param = _teeth[tooth].Param;
            depth = Mathf.Clamp(Mathf.RoundToInt((float)depth / Step) * Step, 0, Settings.AxisMax);
            if (_depths[param] == depth)
                return;
            _depths[param] = depth;
            ValueAssigned?.Invoke(param, depth);
            QueueRedraw();
        }

        void Select(int tooth)
        {
            if (tooth < 0 || tooth >= _teeth.Count || !Round.IsUnknown(_teeth[tooth].Param) || tooth == _selected)
                return;
            _selected = tooth;
            QueueRedraw();
        }

        // the next unknown tooth from the selected one in `direction`, or -1
        int NextUnknown(int direction)
        {
            for (int i = _selected + direction; i >= 0 && i < _teeth.Count; i += direction)
                if (Round.IsUnknown(_teeth[i].Param))
                    return i;
            return -1;
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (_layout == null || _selected < 0)
                return;

            switch (@event)
            {
                case InputEventMouseButton { ButtonIndex: MouseButton.Left } click:
                    _dragging = click.Pressed;
                    if (click.Pressed)
                        CutAt(click.Position, select: true);
                    AcceptEvent();
                    break;
                case InputEventMouseMotion motion when _dragging:
                    CutAt(motion.Position, select: false);
                    AcceptEvent();
                    break;
                case InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.WheelDown or MouseButton.WheelUp } wheel:
                    var tooth = _layout.ToothAt(wheel.Position.X);
                    Select(tooth);
                    if (tooth == _selected)
                        SetDepth(_selected, _depths[_teeth[_selected].Param] + (wheel.ButtonIndex == MouseButton.WheelDown ? Step : -Step));
                    AcceptEvent();
                    break;
                default:
                    if (@event.IsActionPressed("ui_down", allowEcho: true))
                        Nudge(Step);
                    else if (@event.IsActionPressed("ui_up", allowEcho: true))
                        Nudge(-Step);
                    else if (@event.IsActionPressed("ui_left") && NextUnknown(-1) is var left and >= 0)
                        Select(left);
                    else if (@event.IsActionPressed("ui_right") && NextUnknown(1) is var right and >= 0)
                        Select(right);
                    else
                        return; // left/right past the last tooth moves focus on, e.g. to Submit
                    AcceptEvent();
                    break;
            }
        }

        void Nudge(int delta) => SetDepth(_selected, _depths[_teeth[_selected].Param] + delta);

        // the tooth's tip follows the pointer: further down the column is a deeper cut
        void CutAt(Vector2 position, bool select)
        {
            var tooth = select ? _layout.ToothAt(position.X) : _selected;
            if (select)
                Select(tooth);
            if (tooth != _selected)
                return;
            SetDepth(_selected, Mathf.RoundToInt(_layout.DepthAt(position.Y, Settings.AxisMax)));
        }

        int DepthOf(GraphBar tooth) =>
            Round.IsUnknown(tooth.Param) ? _depths[tooth.Param] : Round.Values[tooth.Param];

        public override void _Draw()
        {
            DrawRect(new Rect2(Vector2.Zero, Size), PaperColor);
            if (_teeth.Count == 0)
                return;

            _layout = new Layout(Size, _teeth.Count);
            var l = _layout;
            var font = GetThemeDefaultFont();
            var fontSize = Mathf.Max(10, (int)(Mathf.Min(Size.X, Size.Y) * 0.05f));
            int max = Mathf.Max(Settings.AxisMax, 1);

            // depth scale behind the teeth: a line per step, numbered on the right (0, an uncut
            // tooth, is the blade's edge and would sit under its tip)
            for (int v = 0; v <= max; v += Step)
            {
                var y = l.TipY(v, max);
                DrawLine(new Vector2(l.TeethLeft, y), new Vector2(l.TeethRight, y), GridColor, 1);
                if (v > 0)
                    this.DrawTextCentered(font, v.ToString(), new Vector2(l.TeethRight + fontSize, y), (int)(fontSize * 0.8f), GridColor with { A = 0.8f });
            }

            // bow (a ring) and blade
            DrawCircle(l.BowCentre, l.BowRadius, MetalColor);
            DrawArc(l.BowCentre, l.BowRadius, 0, Mathf.Tau, 48, MetalShadeColor, 3);
            DrawCircle(l.BowCentre, l.BowRadius * 0.45f, PaperColor);
            DrawArc(l.BowCentre, l.BowRadius * 0.45f, 0, Mathf.Tau, 32, MetalShadeColor, 2);
            var blade = new Rect2(l.BladeLeft, l.BladeTop, l.BladeRight - l.BladeLeft, l.BladeHeight);
            DrawRect(blade, MetalColor);
            DrawColoredPolygon([new(l.BladeRight, l.BladeTop), new(l.BladeRight + l.BladeHeight * 0.6f, l.BladeTop + l.BladeHeight / 2),
                new(l.BladeRight, l.BladeTop + l.BladeHeight)], MetalColor);
            DrawLine(new Vector2(l.BladeLeft, l.BladeTop + l.BladeHeight * 0.35f), new Vector2(l.BladeRight, l.BladeTop + l.BladeHeight * 0.35f), MetalShadeColor, 2);

            for (int i = 0; i < _teeth.Count; i++)
            {
                var tooth = _teeth[i];
                bool unknown = Round.IsUnknown(tooth.Param);
                var column = l.Column(i, max);
                var centreX = column.GetCenter().X;
                var depth = DepthOf(tooth);

                if (unknown)
                    this.DrawDashedRect(column, i == _selected && HasFocus() ? SelectedColor : SlotColor, i == _selected ? 3 : 2);

                if (depth > 0)
                {
                    var tipY = l.TipY(depth, max);
                    var half = l.ToothWidth / 2;
                    DrawColoredPolygon([new(centreX - half, l.BladeBottom - 1), new(centreX + half, l.BladeBottom - 1),
                        new(centreX + half * 0.55f, tipY), new(centreX - half * 0.55f, tipY)], unknown ? MetalColor : MetalShadeColor);
                }

                if (Settings.ShowValues)
                    this.DrawTextCentered(font, depth.ToString(), new Vector2(centreX, l.TipY(depth, max) + fontSize), fontSize, LabelColor);
                if (tooth.Label != "")
                    this.DrawTextCentered(font, Round.Format(tooth.Label), new Vector2(centreX, l.LabelY + fontSize), fontSize, LabelColor);
            }
        }

        // Where everything sits for the current size, also used to map the pointer to a tooth and depth.
        sealed class Layout
        {
            public readonly Vector2 BowCentre;
            public readonly float BowRadius, BladeLeft, BladeRight, BladeTop, BladeHeight, TeethLeft, TeethRight, ToothDepth, ToothWidth, LabelY;
            readonly float _slot;
            readonly int _count;

            public float BladeBottom => BladeTop + BladeHeight;

            public Layout(Vector2 size, int count)
            {
                _count = count;
                var margin = Mathf.Min(size.X, size.Y) * 0.06f;
                BowRadius = size.Y * 0.18f;
                BowCentre = new Vector2(margin + BowRadius, size.Y * 0.32f);
                BladeHeight = size.Y * 0.12f;
                BladeTop = BowCentre.Y - BladeHeight / 2;
                BladeLeft = BowCentre.X + BowRadius * 0.8f;
                BladeRight = size.X - margin * 2.5f;
                TeethLeft = BladeLeft + BowRadius * 0.6f;
                TeethRight = BladeRight - BladeHeight;
                _slot = (TeethRight - TeethLeft) / Mathf.Max(count, 1);
                ToothWidth = _slot * 0.7f;
                ToothDepth = size.Y * 0.42f;
                LabelY = BladeBottom + ToothDepth + margin * 0.3f;
            }

            public float TipY(int depth, int max) => BladeBottom + ToothDepth * depth / max;

            public Rect2 Column(int i, int max) =>
                new(TeethLeft + _slot * i + (_slot - ToothWidth) / 2, BladeBottom, ToothWidth, ToothDepth);

            public int ToothAt(float x) => Mathf.Clamp(Mathf.FloorToInt((x - TeethLeft) / _slot), 0, _count - 1);

            public float DepthAt(float y, int max) => Mathf.Clamp((y - BladeBottom) / ToothDepth, 0, 1) * max;
        }
    }
}
