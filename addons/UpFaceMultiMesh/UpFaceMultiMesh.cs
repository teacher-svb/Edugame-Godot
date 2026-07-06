// TODO - [ ] A seed field for reproducible results
// TODO - [ ] A normal threshold slider (currently hardcoded at 0.7) so you can control how steep a slope still gets populated
// TODO - [ ] Append mode vs replace mode, so you can populate multiple surfaces onto the same MultiMesh without losing previous results
// TODO - [ ] An undo step via UndoRedo so Ctrl+Z works after populating


#if TOOLS
using Godot;

[Tool]
public partial class UpFaceMultiMesh : EditorPlugin
{
    private Window _dialog;
    private SpinBox _amount;
    private SpinBox _baseScale;
    private SpinBox _randomScale;
    private SpinBox _randomRotation;
    private SpinBox _randomTilt;
    private SpinBox _minDistance;
    private Button _surfacePickerBtn;
    private MultiMeshInstance3D _selectedMmi;
    private GeometryInstance3D _selectedSurface;

    private PopulatorOptions _lastOptions = new();

    private void OnPopulatePressed()
    {
        SaveSettings();

        try
        {
            new Populator()
                .SelectMultimesh(_selectedMmi)
                .SelectSurface(_selectedSurface)
                .SetOptions(_lastOptions)
                .SetSampler(SamplerType.WeightedByArea)
                .Execute();

            _dialog.Hide();
        }
        catch (System.Exception e)
        {
            GD.PrintErr(e.Message);
            _dialog.Hide();
        }
    }

    private void OnCancelPressed()
    {
        SaveSettings();
        _dialog.Hide();
    }

    private Button _toolbarBtn;

    public override void _EnterTree()
    {
        _toolbarBtn = new Button
        {
            Text = "Populate MultiMesh",
            Visible = false
        };
        _toolbarBtn.Pressed += PopulateUpwardFaces;
        AddControlToContainer(CustomControlContainer.SpatialEditorMenu, _toolbarBtn);

        EditorInterface.Singleton.GetSelection().SelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged()
    {
        var selected = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
        _toolbarBtn.Visible = selected.Count > 0 && selected[0] is MultiMeshInstance3D;
    }

    public override void _ExitTree()
    {
        RemoveControlFromContainer(CustomControlContainer.SpatialEditorMenu, _toolbarBtn);
        _toolbarBtn.QueueFree();
    }

    public override bool _Handles(GodotObject obj)
    {
        _toolbarBtn.Visible = obj is MultiMeshInstance3D;
        return false; // return false so we don't hijack the default editor behavior
    }

    // public override void _EnterTree()
    // {
    //     AddToolMenuItem("Populate Upward Faces", Callable.From(PopulateUpwardFaces));
    // }

    // public override void _ExitTree()
    // {
    //     RemoveToolMenuItem("Populate Upward Faces");
    //     if (_dialog != null) { _dialog.QueueFree(); _dialog = null; }
    // }

    private void PopulateUpwardFaces()
    {
        var selected = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
        _selectedMmi = selected[0] as MultiMeshInstance3D;

        if (_dialog != null) { _dialog.QueueFree(); _dialog = null; }

        _dialog = new Window
        {
            Title = "Populate Upward Faces",
            Size = new Vector2I(400, 450),
        };

        var vbox = new VBoxContainer();
        vbox.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.KeepSize, 8);
        _dialog.AddChild(vbox);

        _surfacePickerBtn = AddNodePicker(vbox, "Surface Mesh", _selectedSurface, OnSurfacePickerPressed);

        vbox.AddChild(new HSeparator());

        _amount = AddField(vbox, "Amount", _lastOptions.Amount, 1, 10000, 1);
        _baseScale = AddField(vbox, "Base Scale", _lastOptions.BaseScale, 0.01f, 100f, 0.1f);
        _randomScale = AddField(vbox, "Random Scale", _lastOptions.RandomScale, 0f, 10f, 0.05f);
        _randomRotation = AddField(vbox, "Random Rotation (deg)", _lastOptions.RandomRotationDeg, 0f, 360f, 1f);
        _randomTilt = AddField(vbox, "Random Tilt (deg)", _lastOptions.RandomTiltDeg, 0f, 90f, 1f);
        _minDistance = AddField(vbox, "Min Distance", _lastOptions.MinDistance, 0f, 100f, 0.1f);

        vbox.AddChild(new HSeparator());

        var hbox = new HBoxContainer();
        vbox.AddChild(hbox);

        var populateBtn = new Button
        {
            Text = "Populate",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        populateBtn.Pressed += OnPopulatePressed;
        hbox.AddChild(populateBtn);

        var cancelBtn = new Button
        {
            Text = "Cancel",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
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

        var lbl = new Label
        {
            Text = label,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        hbox.AddChild(lbl);

        var btn = new Button
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            Text = currentNode != null ? currentNode.Name : "[ empty ]"
        };
        btn.Pressed += onPressed;
        hbox.AddChild(btn);

        return btn;
    }

    private void OnSurfacePickerPressed()
    {
        ShowSceneTreePicker(pickingMmi: false);
    }

    private void OnMmiSelected()
    {
        EditorInterface.Singleton.GetSelection().SelectionChanged -= OnMmiSelected;
        foreach (var node in EditorInterface.Singleton.GetSelection().GetSelectedNodes())
            if (node is MultiMeshInstance3D m) { _selectedMmi = m; break; }
        _dialog.Show();
    }

    private void OnSurfaceSelected()
    {
        EditorInterface.Singleton.GetSelection().SelectionChanged -= OnSurfaceSelected;
        foreach (var node in EditorInterface.Singleton.GetSelection().GetSelectedNodes())
            if (node is MeshInstance3D || node is CsgShape3D)
            {
                _selectedSurface = node as GeometryInstance3D;
                break;
            }
        _surfacePickerBtn.Text = _selectedSurface != null ? _selectedSurface.Name : "[ empty ]";
        _dialog.Show();
    }

    private SpinBox AddField(VBoxContainer vbox, string label, float defaultValue, float min, float max, float step)
    {
        var hbox = new HBoxContainer();
        vbox.AddChild(hbox);

        var lbl = new Label
        {
            Text = label,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        hbox.AddChild(lbl);

        var spinbox = new SpinBox
        {
            MinValue = min,
            MaxValue = max,
            Step = step,
            Value = defaultValue,
            CustomMinimumSize = new Vector2(100, 0)
        };
        hbox.AddChild(spinbox);

        return spinbox;
    }

    private void SaveSettings()
    {
        _lastOptions.Amount = (int)_amount.Value;
        _lastOptions.BaseScale = (float)_baseScale.Value;
        _lastOptions.RandomScale = (float)_randomScale.Value;
        _lastOptions.RandomRotationDeg = (float)_randomRotation.Value;
        _lastOptions.RandomTiltDeg = (float)_randomTilt.Value;
        _lastOptions.MinDistance = (float)_minDistance.Value;
    }


    private void ShowSceneTreePicker(bool pickingMmi)
    {
        var picker = new Window
        {
            Title = "Select Surface Node",
            Size = new Vector2I(350, 500),
            AlwaysOnTop = true,
            PopupWindow = true
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

        tree.ItemSelected += () =>
        {
            var selected = tree.GetSelected();
            if (selected == null) return;
            PickNode(selected.GetMetadata(0).As<Node>(), pickingMmi, picker);
        };

        // Populate tree from scene
        var root = tree.CreateItem();
        PopulateTree(tree, root, EditorInterface.Singleton.GetEditedSceneRoot(), pickingMmi);



        EditorInterface.Singleton.GetBaseControl().AddChild(picker);
        picker.PopupCentered();
        picker.CloseRequested += picker.QueueFree;
    }

    private void PickNode(Node node, bool pickingMmi, Window picker)
    {
        if (pickingMmi && node is MultiMeshInstance3D m)
        {
            _selectedMmi = m;
            picker.QueueFree();
        }
        else if (node is MeshInstance3D || node is CsgShape3D)
        {
            _selectedSurface = node as GeometryInstance3D;
            _surfacePickerBtn.Text = node.Name;
            picker.QueueFree();
        }
        // if invalid type, do nothing — user can try another node
    }

    private void PopulateTree(Tree tree, TreeItem parent, Node node, bool pickingMmi)
    {
        if (node == null) return;

        bool isValid = pickingMmi
            ? node is MultiMeshInstance3D
            : node is MeshInstance3D || node is CsgShape3D;

        var item = tree.CreateItem(parent);
        item.SetText(0, node.Name);
        item.SetMetadata(0, node);

        if (!isValid)
            item.SetCustomColor(0, new Color(0.6f, 0.6f, 0.6f)); // grey out invalid nodes
        else
            item.SetCustomColor(0, new Color(1f, 1f, 1f)); // white for valid

        foreach (var child in node.GetChildren())
            PopulateTree(tree, item, child, pickingMmi);
    }
}
#endif