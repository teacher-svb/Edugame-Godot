using Godot;

namespace TnT.EduGame;

[Tool]
[GlobalClass]
public partial class GPUTrail3D : GpuParticles3D
{
    private ShaderMaterial _drawMaterial;
    private bool _readyDone;

    private int _length = 60;
    [Export]
    public int Length
    {
        get => _length;
        set
        {
            _length = Mathf.Max(value, 1);
            if (!_readyDone) return;
            Amount = _length;
            Lifetime = _length;
            Restart();
        }
    }
    [Export]
    public bool ClearTrail
    {
        get => false;
        set { if (value) Restart(); }
    }

    [ExportCategory("Appearance")]
    private GradientTexture1D _colorRamp;
    [Export]
    public GradientTexture1D ColorRamp
    {
        get => _colorRamp;
        set
        {
            _colorRamp = value;
            _drawMaterial?.SetShaderParameter("color_ramp", _colorRamp);
        }
    }

    private CurveTexture _widthCurve;
    [Export]
    public CurveTexture WidthCurve
    {
        get => _widthCurve;
        set
        {
            _widthCurve = value;
            _drawMaterial?.SetShaderParameter("width_curve", _widthCurve);
        }
    }

    const string ProcessShaderPath = "res://scripts/Trail/trail_process.gdshader";
    const string DrawShaderPath = "res://scripts/Trail/trail_draw.gdshader";

    /// <summary>
    /// Uses the materials saved with the node (made by an earlier <c>_Ready</c> in the editor and stored in the
    /// scene), and only builds them when they are missing or use another shader (a node just added). Rebuilding
    /// them on every <c>_Ready</c> replaced the saved sub-resources each time a scene was opened in the editor.
    /// At run time the draw mesh and its material are copied per node, so two trails sharing them in the scene
    /// (a duplicated node) don't share their colour ramp and width curve.
    /// </summary>
    public override void _Ready()
    {
        Amount = _length;
        Lifetime = _length;
        Explosiveness = 1.0f;
        FixedFps = 60;
        OneShot = false;

        if (!UsesShader(ProcessMaterial as ShaderMaterial, ProcessShaderPath))
        {
            ProcessMaterial = new ShaderMaterial
            {
                Shader = GD.Load<Shader>(ProcessShaderPath)
            };
        }

        if (DrawPass1 is PrimitiveMesh mesh && mesh.Material is ShaderMaterial saved && UsesShader(saved, DrawShaderPath))
        {
            if (Engine.IsEditorHint())
                _drawMaterial = saved;
            else
            {
                _drawMaterial = (ShaderMaterial)saved.Duplicate();
                var copy = (PrimitiveMesh)mesh.Duplicate();
                copy.Material = _drawMaterial;
                DrawPass1 = copy;
            }
        }
        else
        {
            _drawMaterial = new ShaderMaterial
            {
                Shader = GD.Load<Shader>(DrawShaderPath),
                ResourceLocalToScene = true
            };
            DrawPass1 = new QuadMesh { Material = _drawMaterial };
        }

        _readyDone = true;

        // flush pending inspector values into shader
        ColorRamp = _colorRamp;
        WidthCurve = _widthCurve;
    }

    static bool UsesShader(ShaderMaterial material, string path) =>
        material?.Shader != null && material.Shader.ResourcePath == path;
}
