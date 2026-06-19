#if TOOLS
using Godot;

[Tool]
public partial class BezierPathGenerator : EditorPlugin
{
    private Button _toolbarBtn;
    private Window _dialog;
    private OptionButton _typeSelector;
    private VBoxContainer _paramsContainer;
    private Path3D _selectedPath;

    private SpinBox _paramLength;
    private SpinBox _paramHeight;
    private SpinBox _paramOffset;
    private SpinBox _paramRadius;
    private SpinBox _paramDiameter;
    private SpinBox _paramGap;
    private SpinBox _paramAmplitude;
    private SpinBox _paramCycles;
    private SpinBox _paramTurns;
    private OptionButton _paramPlane;

    private enum CurveType
    {
        Straight,
        Parabolic,
        SCurve,
        Circle,
        SemiCircle,
        Loop,
        Wave,
        Spiral,
    }

    public override void _EnterTree()
    {
        _toolbarBtn = new Button
        {
            Text = "Generate Curve",
            Visible = false
        };
        _toolbarBtn.Pressed += OpenDialog;
        AddControlToContainer(CustomControlContainer.SpatialEditorMenu, _toolbarBtn);
        EditorInterface.Singleton.GetSelection().SelectionChanged += OnSelectionChanged;
    }

    public override void _ExitTree()
    {
        RemoveControlFromContainer(CustomControlContainer.SpatialEditorMenu, _toolbarBtn);
        _toolbarBtn.QueueFree();
        EditorInterface.Singleton.GetSelection().SelectionChanged -= OnSelectionChanged;
    }

    public override bool _Handles(GodotObject obj)
    {
        _toolbarBtn.Visible = obj is Path3D;
        return false;
    }

    private void OnSelectionChanged()
    {
        var selected = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
        _toolbarBtn.Visible = selected.Count > 0 && selected[0] is Path3D;
    }

    private void OpenDialog()
    {
        var selected = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
        _selectedPath = selected.Count > 0 ? selected[0] as Path3D : null;
        if (_selectedPath == null) return;

        if (_dialog != null) { _dialog.QueueFree(); _dialog = null; }

        _dialog = new Window
        {
            Title = $"Generate Curve — {_selectedPath.Name}",
            Size = new Vector2I(360, 320),
        };

        var vbox = new VBoxContainer();
        vbox.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.KeepSize, 8);
        _dialog.AddChild(vbox);

        var typeRow = new HBoxContainer();
        vbox.AddChild(typeRow);
        typeRow.AddChild(new Label
        {
            Text = "Curve Type",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        });

        _typeSelector = new OptionButton { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        foreach (var name in System.Enum.GetNames(typeof(CurveType)))
            _typeSelector.AddItem(name);
        _typeSelector.ItemSelected += _ => RebuildParams();
        typeRow.AddChild(_typeSelector);

        vbox.AddChild(new HSeparator());

        _paramsContainer = new VBoxContainer();
        vbox.AddChild(_paramsContainer);

        // spacer so buttons stay at bottom
        var spacer = new Control();
        spacer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        vbox.AddChild(spacer);

        vbox.AddChild(new HSeparator());

        var btnRow = new HBoxContainer();
        vbox.AddChild(btnRow);

        var generateBtn = new Button
        {
            Text = "Generate",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        generateBtn.Pressed += OnGeneratePressed;
        btnRow.AddChild(generateBtn);

        var cancelBtn = new Button
        {
            Text = "Cancel",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        cancelBtn.Pressed += OnCancelPressed;
        btnRow.AddChild(cancelBtn);

        EditorInterface.Singleton.GetBaseControl().AddChild(_dialog);
        _dialog.CloseRequested += OnCancelPressed;
        _dialog.PopupCentered();

        RebuildParams();
    }

    private void OnCancelPressed()
    {
        _dialog?.Hide();
    }

    private void RebuildParams()
    {
        foreach (Node child in _paramsContainer.GetChildren())
        {
            _paramsContainer.RemoveChild(child);
            child.Free();
        }

        _paramLength = _paramHeight = _paramOffset = _paramRadius =
            _paramDiameter = _paramGap = _paramAmplitude = _paramCycles = _paramTurns = null;
        _paramPlane = null;

        switch ((CurveType)_typeSelector.Selected)
        {
            case CurveType.Straight:
                _paramLength = AddSpinRow("Length", 10f, 0.1f, 1000f, 0.5f);
                break;

            case CurveType.Parabolic:
                _paramLength = AddSpinRow("Length", 10f, 0.1f, 1000f, 0.5f);
                _paramHeight = AddSpinRow("Height", 5f, 0.1f, 500f, 0.5f);
                break;

            case CurveType.SCurve:
                _paramLength = AddSpinRow("Length", 10f, 0.1f, 1000f, 0.5f);
                break;

            case CurveType.Circle:
                _paramRadius = AddSpinRow("Radius", 5f, 0.1f, 500f, 0.5f);
                _paramPlane = AddPlaneRow();
                break;

            case CurveType.SemiCircle:
                _paramRadius = AddSpinRow("Radius", 5f, 0.1f, 500f, 0.5f);
                _paramPlane = AddPlaneRow();
                break;

            case CurveType.Loop:
                _paramLength = AddSpinRow("Length", 10f, 0.1f, 1000f, 0.5f);
                _paramDiameter = AddSpinRow("Loop Diameter", 6f, 0.1f, 200f, 0.5f);
                break;

            case CurveType.Wave:
                _paramLength = AddSpinRow("Length", 20f, 0.1f, 1000f, 0.5f);
                _paramAmplitude = AddSpinRow("Amplitude", 3f, 0.1f, 200f, 0.25f);
                _paramCycles = AddSpinRow("Cycles", 2f, 1f, 32f, 1f);
                break;

            case CurveType.Spiral:
                _paramLength = AddSpinRow("Length", 20f, 0.1f, 1000f, 0.5f);
                _paramRadius = AddSpinRow("Radius", 3f, 0.1f, 200f, 0.25f);
                _paramTurns = AddSpinRow("Turns", 2f, 1f, 32f, 1f);
                break;
        }
    }

    private void OnGeneratePressed()
    {
        if (_selectedPath == null) return;

        Curve3D curve = BuildCurve((CurveType)_typeSelector.Selected);
        if (curve == null) return;

        var undo = GetUndoRedo();
        undo.CreateAction($"Generate {(CurveType)_typeSelector.Selected} curve");
        undo.AddDoProperty(_selectedPath, "curve", curve);
        undo.AddUndoProperty(_selectedPath, "curve", _selectedPath.Curve ?? new Curve3D());
        undo.CommitAction();

        _dialog.Hide();
    }

    private Curve3D BuildCurve(CurveType type)
    {
        float length = (float)(_paramLength?.Value ?? 10.0);
        Vector3 start = Vector3.Zero;
        Vector3 end = new Vector3(length, 0f, 0f);

        return type switch
        {
            CurveType.Straight => Beziers.Straight(start, end),
            CurveType.Parabolic => Beziers.Parabolic(start, end, (float)_paramHeight.Value),
            CurveType.SCurve => Beziers.SCurve(start, end),
            CurveType.Circle => BuildCircle(),
            CurveType.SemiCircle => BuildSemiCircle(),
            CurveType.Loop => Beziers.Looping(start, end, (float)_paramDiameter.Value, (float)_paramDiameter.Value),
            CurveType.Wave => Beziers.Wave(start, end, (float)_paramAmplitude.Value, (int)_paramCycles.Value),
            CurveType.Spiral => Beziers.Spiral(start, end, (float)_paramRadius.Value, (int)_paramTurns.Value),
            _ => null,
        };
    }

    private Curve3D BuildCircle()
    {
        float r = (float)_paramRadius.Value;
        Vector3 normal = _paramPlane.Selected switch
        {
            0 => Vector3.Forward,  // XY vertical front
            1 => Vector3.Up,    // XZ horizontal
            _ => Vector3.Right, // YZ vertical side
        };
        return Beziers.Circle(Vector3.Zero, r, normal);
    }

    private Curve3D BuildSemiCircle()
    {
        float r = (float)_paramRadius.Value;
        Vector3 normal = _paramPlane.Selected switch
        {
            0 => Vector3.Back,  // XY vertical front
            1 => Vector3.Up,    // XZ horizontal
            _ => Vector3.Right, // YZ vertical side
        };
        return Beziers.SemiCircle(Vector3.Zero, r, normal);
    }

    private SpinBox AddSpinRow(string label, float defaultValue, float min, float max, float step)
    {
        var row = new HBoxContainer();
        _paramsContainer.AddChild(row);

        row.AddChild(new Label
        {
            Text = label,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        });

        var spin = new SpinBox
        {
            MinValue = min,
            MaxValue = max,
            Step = step,
            Value = defaultValue,
            CustomMinimumSize = new Vector2(110, 0)
        };
        row.AddChild(spin);

        return spin;
    }

    private OptionButton AddPlaneRow()
    {
        var row = new HBoxContainer();
        _paramsContainer.AddChild(row);

        row.AddChild(new Label
        {
            Text = "Plane",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        });

        var opt = new OptionButton { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        opt.AddItem("XY  (vertical front)");
        opt.AddItem("XZ  (horizontal)");
        opt.AddItem("YZ  (vertical side)");
        row.AddChild(opt);

        return opt;
    }
}
#endif
