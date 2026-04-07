#if TOOLS
using Godot;

// TODO - [ ] A seed field for reproducible results
// TODO - [ ] A normal threshold slider (currently hardcoded at 0.7) so you can control how steep a slope still gets populated
// TODO - [ ] Append mode vs replace mode, so you can populate multiple surfaces onto the same MultiMesh without losing previous results
// TODO - [ ] An undo step via UndoRedo so Ctrl+Z works after populating

[Tool]
public partial class UpFaceMultiMesh : EditorPlugin
{
    private Window _dialog;
    private SpinBox _amount;
    private SpinBox _baseScale;
    private SpinBox _randomScale;
    private SpinBox _randomRotation;
    private SpinBox _randomTilt;
    private Button _mmiPickerBtn;
    private Button _surfacePickerBtn;
    private MultiMeshInstance3D _selectedMmi;
    private MeshInstance3D _selectedSurface;

    // Remembered settings
    private float _lastAmount = 128;
    private float _lastBaseScale = 1.0f;
    private float _lastRandomScale = 0.3f;
    private float _lastRandomRotation = 360f;
    private float _lastRandomTilt = 0f;

    public override void _EnterTree()
    {
        AddToolMenuItem("Populate Upward Faces", Callable.From(PopulateUpwardFaces));
    }

    public override void _ExitTree()
    {
        RemoveToolMenuItem("Populate Upward Faces");
        if (_dialog != null)
        {
            _dialog.QueueFree();
            _dialog = null;
        }
    }

    private void PopulateUpwardFaces()
    {
        var selected = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
        foreach (var node in selected)
        {
            if (node is MultiMeshInstance3D m) _selectedMmi = m;
            if (node is MeshInstance3D s) _selectedSurface = s;
        }

        if (_dialog != null)
        {
            _dialog.QueueFree();
            _dialog = null;
        }

        _dialog = new Window();
        _dialog.Title = "Populate Upward Faces";
        _dialog.Size = new Vector2I(400, 420);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.KeepSize, 8);
        _dialog.AddChild(vbox);

        _mmiPickerBtn = AddNodePicker(vbox, "MultiMeshInstance3D", _selectedMmi, OnMmiPickerPressed);
        _surfacePickerBtn = AddNodePicker(vbox, "Surface Mesh", _selectedSurface, OnSurfacePickerPressed);

        vbox.AddChild(new HSeparator());

        _amount         = AddField(vbox, "Amount",                _lastAmount,         1,     10000, 1);
        _baseScale      = AddField(vbox, "Base Scale",            _lastBaseScale,      0.01f, 100f,  0.1f);
        _randomScale    = AddField(vbox, "Random Scale",          _lastRandomScale,    0f,    10f,   0.05f);
        _randomRotation = AddField(vbox, "Random Rotation (deg)", _lastRandomRotation, 0f,    360f,  1f);
        _randomTilt     = AddField(vbox, "Random Tilt (deg)",     _lastRandomTilt,     0f,    90f,   1f);

        vbox.AddChild(new HSeparator());

        var hbox = new HBoxContainer();
        vbox.AddChild(hbox);

        var populateBtn = new Button();
        populateBtn.Text = "Populate";
        populateBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        populateBtn.Pressed += OnPopulatePressed;
        hbox.AddChild(populateBtn);

        var cancelBtn = new Button();
        cancelBtn.Text = "Cancel";
        cancelBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        cancelBtn.Pressed += OnCancelPressed;
        hbox.AddChild(cancelBtn);

        EditorInterface.Singleton.GetBaseControl().AddChild(_dialog);
        _dialog.PopupCentered();
        _dialog.CloseRequested += OnCancelPressed;
    }

    private Button AddNodePicker(VBoxContainer vbox, string label, Node currentNode, System.Action onPressed)
    {
        var hbox = new HBoxContainer();
        vbox.AddChild(hbox);

        var lbl = new Label();
        lbl.Text = label;
        lbl.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(lbl);

        var btn = new Button();
        btn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        btn.Text = currentNode != null ? currentNode.Name : "[ empty ]";
        btn.Pressed += onPressed;
        hbox.AddChild(btn);

        return btn;
    }

    private void OnMmiPickerPressed()
    {
        _dialog.Hide();
        EditorInterface.Singleton.GetSelection().SelectionChanged += OnMmiSelected;
    }

    private void OnMmiSelected()
    {
        EditorInterface.Singleton.GetSelection().SelectionChanged -= OnMmiSelected;
        var selected = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
        foreach (var node in selected)
        {
            if (node is MultiMeshInstance3D m)
            {
                _selectedMmi = m;
                break;
            }
        }
        _mmiPickerBtn.Text = _selectedMmi != null ? _selectedMmi.Name : "[ empty ]";
        _dialog.Show();
    }

    private void OnSurfacePickerPressed()
    {
        _dialog.Hide();
        EditorInterface.Singleton.GetSelection().SelectionChanged += OnSurfaceSelected;
    }

    private void OnSurfaceSelected()
    {
        EditorInterface.Singleton.GetSelection().SelectionChanged -= OnSurfaceSelected;
        var selected = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
        foreach (var node in selected)
        {
            if (node is MeshInstance3D s)
            {
                _selectedSurface = s;
                break;
            }
        }
        _surfacePickerBtn.Text = _selectedSurface != null ? _selectedSurface.Name : "[ empty ]";
        _dialog.Show();
    }

    private SpinBox AddField(VBoxContainer vbox, string label, float defaultValue, float min, float max, float step)
    {
        var hbox = new HBoxContainer();
        vbox.AddChild(hbox);

        var lbl = new Label();
        lbl.Text = label;
        lbl.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(lbl);

        var spinbox = new SpinBox();
        spinbox.MinValue = min;
        spinbox.MaxValue = max;
        spinbox.Step = step;
        spinbox.Value = defaultValue;
        spinbox.CustomMinimumSize = new Vector2(100, 0);
        hbox.AddChild(spinbox);

        return spinbox;
    }

    private void SaveSettings()
    {
        _lastAmount = (float)_amount.Value;
        _lastBaseScale = (float)_baseScale.Value;
        _lastRandomScale = (float)_randomScale.Value;
        _lastRandomRotation = (float)_randomRotation.Value;
        _lastRandomTilt = (float)_randomTilt.Value;
    }

    private void OnPopulatePressed()
    {
        if (_selectedMmi == null || _selectedSurface == null)
        {
            GD.PrintErr("Select both a MultiMeshInstance3D and a surface MeshInstance3D.");
            return;
        }

        if (_selectedMmi.Multimesh == null || _selectedMmi.Multimesh.Mesh == null)
        {
            GD.PrintErr("MultiMeshInstance3D has no mesh assigned.");
            return;
        }

        SaveSettings();

        PopulateOnUpwardFaces(
            _selectedMmi,
            _selectedSurface,
            amount: (int)_lastAmount,
            normalThreshold: 0.7f,
            baseScale: _lastBaseScale,
            randomScale: _lastRandomScale,
            randomRotationDeg: _lastRandomRotation,
            randomTiltDeg: _lastRandomTilt
        );

        _dialog.Hide();
    }

    private void OnCancelPressed()
    {
        SaveSettings();
        _dialog.Hide();
    }

    private void PopulateOnUpwardFaces(
        MultiMeshInstance3D mmi,
        MeshInstance3D surface,
        int amount,
        float normalThreshold,
        float baseScale,
        float randomScale,
        float randomRotationDeg,
        float randomTiltDeg)
    {
        var faces = surface.Mesh.GetFaces();
        var upFaces = new System.Collections.Generic.List<(Vector3, Vector3, Vector3)>();
        var upFaceAreas = new System.Collections.Generic.List<float>();
        float totalArea = 0f;

        for (int i = 0; i < faces.Length - 2; i += 3)
        {
            var a = faces[i];
            var b = faces[i + 1];
            var c = faces[i + 2];
            var normal = (b - a).Cross(c - a).Normalized();
            var worldNormal = (surface.GlobalTransform.Basis * -normal).Normalized();
            if (worldNormal.Dot(Vector3.Up) > normalThreshold)
            {
                float area = (b - a).Cross(c - a).Length() * 0.5f;
                upFaces.Add((a, b, c));
                upFaceAreas.Add(area);
                totalArea += area;
            }
        }

        if (upFaces.Count == 0)
        {
            GD.PrintErr("No upward-facing faces found.");
            return;
        }

        // Build cumulative area array for weighted random selection
        var cumulativeAreas = new float[upFaceAreas.Count];
        float cumulative = 0f;
        for (int i = 0; i < upFaceAreas.Count; i++)
        {
            cumulative += upFaceAreas[i];
            cumulativeAreas[i] = cumulative;
        }

        var mm = new MultiMesh();
        mm.Mesh = mmi.Multimesh?.Mesh;
        mm.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        mm.InstanceCount = amount;

        var rng = new RandomNumberGenerator();
        rng.Randomize();

        for (int i = 0; i < amount; i++)
        {
            // Pick triangle weighted by area
            float r = rng.Randf() * totalArea;
            int faceIndex = 0;
            for (int j = 0; j < cumulativeAreas.Length; j++)
            {
                if (r <= cumulativeAreas[j])
                {
                    faceIndex = j;
                    break;
                }
            }

            var face = upFaces[faceIndex];
            var point = RandomPointOnTriangle(face.Item1, face.Item2, face.Item3, rng);
            point = surface.GlobalTransform * point;
            point = mmi.GlobalTransform.AffineInverse() * point;

            float scale = baseScale + rng.RandfRange(-randomScale, randomScale);
            float rotY = rng.RandfRange(0, Mathf.DegToRad(randomRotationDeg));
            float tiltAngle = rng.RandfRange(0, Mathf.DegToRad(randomTiltDeg));
            float tiltDir = rng.RandfRange(0, Mathf.Tau);

            var t = Transform3D.Identity;
            t = t.Rotated(Vector3.Up, rotY);
            t = t.Rotated(new Vector3(Mathf.Cos(tiltDir), 0, Mathf.Sin(tiltDir)), tiltAngle);
            t = t.Scaled(Vector3.One * scale);
            t.Origin = point;

            mm.SetInstanceTransform(i, t);
        }

        mmi.Multimesh = mm;
        GD.Print($"Populated {amount} instances on {upFaces.Count} upward faces.");
    }

    private Vector3 RandomPointOnTriangle(Vector3 a, Vector3 b, Vector3 c, RandomNumberGenerator rng)
    {
        float r1 = Mathf.Sqrt(rng.Randf());
        float r2 = rng.Randf();
        return a * (1 - r1) + b * (r1 * (1 - r2)) + c * (r1 * r2);
    }
}
#endif