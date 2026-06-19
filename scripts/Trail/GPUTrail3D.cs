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

    public override void _Ready()
    {
        Amount = _length;
        Lifetime = _length;
        Explosiveness = 1.0f;
        FixedFps = 60;
        OneShot = false;

        var processMat = new ShaderMaterial
        {
            Shader = GD.Load<Shader>("res://scripts/Trail/trail_process.gdshader")
        };
        ProcessMaterial = processMat;

        _drawMaterial = new ShaderMaterial
        {
            Shader = GD.Load<Shader>("res://scripts/Trail/trail_draw.gdshader"),
            ResourceLocalToScene = true
        };

        var mesh = new QuadMesh { Material = _drawMaterial };
        DrawPass1 = mesh;

        _readyDone = true;

        // flush pending inspector values into shader
        ColorRamp = _colorRamp;
        WidthCurve = _widthCurve;
    }
}
