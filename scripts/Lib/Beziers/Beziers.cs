using Godot;
using System;
using System.Linq;
using TnT.Extensions;

public static class Beziers
{
    private const float Kappa = 0.5522847498f; // 4/3 * tan(π/8) — best 4-segment circle approximation

    // ─── Flat / 2-D shapes ────────────────────────────────────────────────────

    /// <summary>
    /// Straight line from start to end with smooth tangent handles.
    /// </summary>
    public static Curve3D Straight(Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 dir = (endPoint - startPoint) / 3f;
        var curve = new Curve3D();
        curve.AddPoint(startPoint, Vector3.Zero, dir);
        curve.AddPoint(endPoint, -dir, Vector3.Zero);
        return curve;
    }

    /// <summary>
    /// Parabolic arch from start to end, peaking at height above the midpoint.
    /// height is measured along world up.
    /// </summary>
    public static Curve3D Parabolic(Vector3 startPoint, Vector3 endPoint, float height = 5f)
    {
        Vector3 span = endPoint - startPoint;
        Vector3 cpX = span / 3f;
        // 4/3 * height gives the correct bezier control offset for a parabola
        Vector3 cpY = Vector3.Up * (height * 4f / 3f);

        var curve = new Curve3D();
        curve.AddPoint(startPoint, Vector3.Zero, cpX + cpY);
        curve.AddPoint(endPoint, -cpX + cpY, Vector3.Zero);
        return curve;
    }

    /// <summary>
    /// S-curve from start to end, deflecting sideways by offset (local right of the path).
    /// </summary>
    public static Curve3D SCurve(Vector3 startPoint, Vector3 endPoint)
    {
        var curve = new Curve3D();

        var semiCircle1 = SemiEllipse(
            center: startPoint + endPoint * 0.25f,
            width: startPoint.DistanceTo(endPoint) / 2,
            height: startPoint.DistanceTo(endPoint) / 2,
            normal: Vector3.Back);

        var semiCircle2 = SemiEllipse(
            center: startPoint + endPoint * 0.75f,
            width: startPoint.DistanceTo(endPoint) / 2,
            height: startPoint.DistanceTo(endPoint) / 2,
            normal: Vector3.Forward,
            2);


        Curve3DPoint[] stitched = [
            .. semiCircle1,
            .. semiCircle2,
            ];

        stitched.ForEach(p => curve.AddPoint(p.Position, p.In, p.Out));

        return curve;
    }

    // ─── Closed shapes ────────────────────────────────────────────────────────

    /// <summary>
    /// Full circle in the plane defined by normal. Starts and ends at center + right * radius.
    /// </summary>
    public static Curve3D Circle(Vector3 center, float radius, Vector3 normal)
    {
        var curve = new Curve3D();
        Ellipse(center, radius * 2f, radius * 2f, normal)
            .ForEach(p => curve.AddPoint(p.Position, p.In, p.Out));
        return curve;
    }

    // ─── Closed shapes ────────────────────────────────────────────────────────

    /// <summary>
    /// Full circle in the plane defined by normal. Starts and ends at center + right * radius.
    /// </summary>
    public static Curve3D SemiCircle(Vector3 center, float radius, Vector3 normal)
    {
        var curve = new Curve3D();
        SemiEllipse(center, radius * 2f, radius * 2f, normal)
            .ForEach(p => curve.AddPoint(p.Position, p.In, p.Out));
        return curve;
    }

    // ─── Path-through shapes ──────────────────────────────────────────────────

    /// <summary>
    /// Path that enters the bottom of a vertical loop, traverses it, and exits.
    /// startPoint and endPoint define the bottom of the loop.
    /// </summary>
    public static Curve3D Looping(
        Vector3 startPoint,
        Vector3 endPoint,
        float loopingDiameter)
    {

        var curve = new Curve3D();

        curve.AddPoint(startPoint, Vector3.Zero, Vector3.Forward * loopingDiameter * 0.5f);

        Ellipse(
            center: (startPoint + endPoint) * 0.5f + Vector3.Up * loopingDiameter * 0.5f,
            width: loopingDiameter * 0.5f,
            height: loopingDiameter * 0.5f,
            normal: Vector3.Forward)
            .ForEach(p => curve.AddPoint(p.Position, p.In, p.Out));

        curve.AddPoint(endPoint, Vector3.Back * loopingDiameter * 0.5f, Vector3.Zero);

        return curve;
    }

    /// <summary>
    /// Path through two consecutive vertical loops with a flat gap between them.
    /// startPoint / endPoint are the bottom entry / exit of the double-loop section.
    /// </summary>
    public static Curve3D DoubleLooping(
        Vector3 startPoint,
        Vector3 endPoint,
        float loopingDiameter,
        float gap = 2f)
    {
        float radius = loopingDiameter * 0.5f;
        float k = Kappa * radius;

        Vector3 flatDir = endPoint - startPoint;
        flatDir.Y = 0f;
        if (flatDir.LengthSquared() < 0.0001f) flatDir = Vector3.Forward;
        flatDir = flatDir.Normalized();

        Vector3 up = Vector3.Up;

        Vector3 c1 = startPoint + flatDir * radius + up * radius;
        Vector3 c2 = c1 + flatDir * (2f * radius + gap);

        var curve = new Curve3D();

        AddLoopPoints(curve, c1, radius, k, flatDir, up);

        // bridge between loops — handles encode the gap length
        float bridgeK = gap * 0.333f;
        Vector3 loop2EntryBottom = c2 - flatDir * radius - up * radius;

        // patch the out-handle of the last loop1 point and add bridge point
        // last point added is exitSide of loop1 (center + flatDir*r), heading down (out = -up*k)
        // we need it to instead head toward loop2 entry
        curve.SetPointOut(curve.PointCount - 1, flatDir * bridgeK - up * k);
        curve.AddPoint(loop2EntryBottom, @in: -flatDir * bridgeK, @out: flatDir * k);

        AddLoopPoints(curve, c2, radius, k, flatDir, up);

        return curve;
    }

    // ─── Repeating shapes ─────────────────────────────────────────────────────

    /// <summary>
    /// Sine-wave approximation from start to end. cycles controls how many full waves to fit.
    /// amplitude offsets perpendicular to the path in world up.
    /// </summary>
    public static Curve3D Wave(Vector3 startPoint, Vector3 endPoint, float amplitude = 3f, int cycles = 2)
    {
        int halfCycles = cycles * 2;
        Vector3 span = endPoint - startPoint;
        Vector3 step = span / halfCycles;

        // π/3 control offset gives best sine approximation for half-arc bezier
        float cpy = amplitude * Mathf.Pi / 3f;
        Vector3 cpX = step / 3f;
        Vector3 cpUp = Vector3.Up * cpy;

        var curve = new Curve3D();

        for (int i = 0; i <= halfCycles; i++)
        {
            Vector3 pos = startPoint + step * i;
            // odd half-cycles go negative
            float sign = (i % 2 == 0) ? 1f : -1f;
            float prevSign = -sign;

            Vector3 inHandle = (i == 0) ? Vector3.Zero : -cpX + cpUp * prevSign;
            Vector3 outHandle = (i == halfCycles) ? Vector3.Zero : cpX + cpUp * sign;

            curve.AddPoint(pos, inHandle, outHandle);
        }

        return curve;
    }

    /// <summary>
    /// Helix (constant-radius spiral) from start to end.
    /// The circle is in the YZ plane and progresses along the start→end axis.
    /// </summary>
    public static Curve3D Spiral(Vector3 startPoint, Vector3 endPoint, float radius = 3f, int turns = 2)
    {
        int steps = turns * 4;
        Vector3 axis = (endPoint - startPoint) / steps;
        float axisK = axis.Length() / 3f;
        float circleK = Kappa * radius;

        // build two orthogonal vectors perpendicular to axis
        Vector3 axisDir = axis.Normalized();
        Vector3 t1 = axisDir.Cross(Vector3.Up);
        if (t1.LengthSquared() < 0.001f) t1 = axisDir.Cross(Vector3.Right);
        t1 = t1.Normalized();
        Vector3 t2 = axisDir.Cross(t1).Normalized();

        var curve = new Curve3D();

        for (int i = 0; i <= steps; i++)
        {
            float angle = i * Mathf.Pi * 0.5f;
            float s = Mathf.Sin(angle);
            float c = Mathf.Cos(angle);

            Vector3 pos = startPoint + axis * i + (t1 * c + t2 * s) * radius;

            // tangent in the circle plane: d/dθ(c*t1 + s*t2) = -s*t1 + c*t2
            Vector3 circleTangent = (-t1 * s + t2 * c) * circleK;
            Vector3 axialTangent = axisDir * axisK;

            Vector3 outHandle = (i == steps) ? Vector3.Zero : circleTangent + axialTangent;
            Vector3 inHandle = (i == 0) ? Vector3.Zero : -circleTangent - axialTangent;

            curve.AddPoint(pos, inHandle, outHandle);
        }

        return curve;
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private struct Curve3DPoint
    {
        public Vector3 Position;
        public Vector3 In;
        public Vector3 Out;

        public Curve3DPoint(Vector3 position, Vector3 @in, Vector3 @out)
        {
            Position = position;
            In = @in;
            Out = @out;
        }
    }

    private static void AddLoopPoints(Curve3D curve, Vector3 center, float radius, float k, Vector3 flatDir, Vector3 up)
    {
        Vector3 bottom = center - up * radius;
        Vector3 topPos = center + up * radius;
        Vector3 entrySide = center - flatDir * radius;
        Vector3 exitSide = center + flatDir * radius;

        curve.AddPoint(bottom, @in: -flatDir * k, @out: flatDir * k);
        curve.AddPoint(entrySide, @in: -up * k, @out: up * k);
        curve.AddPoint(topPos, @in: flatDir * k, @out: -flatDir * k);
        curve.AddPoint(exitSide, @in: up * k, @out: -up * k);
    }

    // rotation steps: 0=bottom, 1=right, 2=top, 3=left
    private static Curve3DPoint[] Ellipse(Vector3 center, float width, float height, Vector3 normal, int rotation = 0)
    {
        normal = normal.Normalized();

        Vector3 t1 = normal.Cross(Vector3.Up);
        if (t1.LengthSquared() < 0.001f)
            t1 = normal.Cross(Vector3.Right);
        t1 = t1.Normalized();
        Vector3 t2 = normal.Cross(t1).Normalized();

        float hw = width * 0.5f;
        float hh = height * 0.5f;
        float kW = Kappa * hw;
        float kH = Kappa * hh;

        Vector3 p0 = center + t2 * hh;
        Vector3 p1 = center + t1 * hw;
        Vector3 p2 = center - t2 * hh;
        Vector3 p3 = center - t1 * hw;

        Curve3DPoint[] points =
        [
            new Curve3DPoint(p0, @in: -t1 * kW, @out:  t1 * kW),  // bottom → right
            new Curve3DPoint(p1, @in:  t2 * kH, @out: -t2 * kH),  // right  → up
            new Curve3DPoint(p2, @in:  t1 * kW, @out: -t1 * kW),  // top    → left
            new Curve3DPoint(p3, @in: -t2 * kH, @out:  t2 * kH),  // left   → down
        ];

        var rotated = points.Rotate(rotation).ToArray();
        return [.. rotated, rotated[0]];
    }

    // rotation steps: 0=bottom, 1=right, 2=top, 3=left
    private static Curve3DPoint[] SemiEllipse(Vector3 center, float width, float height, Vector3 normal, int rotation = 0)
    {
        var points = Ellipse(center, width, height, normal, rotation);

        return [.. points.Take(1..4)];
    }
}
