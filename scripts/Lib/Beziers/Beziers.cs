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
    public static Curve3D SCurve(Vector3 startPoint, Vector3 endPoint, Vector3 normal)
    {
        normal = normal.Normalized();

        var dir = endPoint - startPoint;
        var distance = dir.Length();
        var radius = distance / 4f;
        var right = normal.Cross(dir.Normalized()).Normalized();

        var curve = new Curve3D();
        var first = GetSemiEllipse(
            startPoint,
            -right,
            normal,
            radius * 2f,
            radius * 2f
        );
        var second = GetSemiEllipse(
            first[^1].Position,
            first[^1].Out,
            normal * -1,
            radius * 2f,
            radius * 2f
        );

        Curve3DPoint[] points = [.. first, .. second.Skip(1)];
        points.ForEach(p => curve.AddPoint(p.Position, p.In, p.Out));

        return curve;
    }

    // ─── Closed shapes ────────────────────────────────────────────────────────

    /// <summary>
    /// Full circle in the plane defined by normal. Starts and ends at center + right * radius.
    /// </summary>
    public static Curve3D Circle(Vector3 center, float radius, Vector3 normal)
    {
        var startPoint = CircleStartPoint(center, radius * 2f, normal);

        var curve = new Curve3D();
        GetEllipse(startPoint.Position, startPoint.Out, normal, radius * 2f, radius * 2f)
            .ForEach(p => curve.AddPoint(p.Position, p.In, p.Out));

        return curve;
    }

    // ─── Closed shapes ────────────────────────────────────────────────────────

    /// <summary>
    /// Full circle in the plane defined by normal. Starts and ends at center + right * radius.
    /// </summary>
    public static Curve3D SemiCircle(Vector3 center, float radius, Vector3 normal)
    {
        var startPoint = CircleStartPoint(center, radius * 2f, normal);

        var curve = new Curve3D();
        GetSemiEllipse(
            startPoint.Position,
            startPoint.Out,
            normal,
            radius * 2f,
            radius * 2f
        )
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
        float loopWidth,
        float loopHeight)
    {

        var curve = new Curve3D();

        var dir = (endPoint - startPoint).Normalized();

        curve.AddPoint(startPoint, Vector3.Zero, dir);

        GetEllipse(
            startPoint: (startPoint + endPoint) * 0.5f,
            startPointTangent: dir,
            normal: Vector3.Back,
            width: loopWidth,
            height: loopHeight
        )
            .ForEach(p => curve.AddPoint(p.Position, p.In, p.Out));

        curve.AddPoint(endPoint, dir * -1, Vector3.Zero);

        return curve;
    }

    // ─── Repeating shapes ─────────────────────────────────────────────────────

    /// <summary>
    /// Sine-wave approximation from start to end. cycles controls how many full waves to fit.
    /// amplitude offsets perpendicular to the path in world up.
    /// </summary>
    public static Curve3D Wave(Vector3 startPoint, Vector3 endPoint, Vector3 normal, float amplitude = 3f, int cycles = 2)
    {
        normal = normal.Normalized();

        var span = endPoint - startPoint;
        var distance = span.Length();

        var cycleLength = distance / cycles;
        var semiAdvance = cycleLength / 2f;
        var semiSpan = amplitude * 2f;

        var dir = span.Normalized();
        var right = normal.Cross(dir).Normalized();

        Curve3DPoint[] points = [new Curve3DPoint(startPoint, Vector3.Zero, -right * Kappa * (semiSpan / 2f))];

        for (int i = 0; i < cycles; i++)
        {
            var firstHalf = GetSemiEllipse(
                points[^1].Position,
                points[^1].Out,
                normal,
                semiSpan,
                semiAdvance
            );
            var secondHalf = GetSemiEllipse(
                firstHalf[^1].Position,
                firstHalf[^1].Out,
                normal * -1,
                semiSpan,
                semiAdvance
            );

            points = [.. points, .. firstHalf.Skip(1), .. secondHalf.Skip(1)];
        }

        var curve = new Curve3D();
        points.ForEach(p => curve.AddPoint(p.Position, p.In, p.Out));

        return curve;
    }

    /// <summary>
    /// Helix (constant-radius spiral) from start to end.
    /// The circle is in the YZ plane and progresses along the start→end axis.
    /// </summary>
    public static Curve3D Spiral(Vector3 startPoint, Vector3 endPoint, float radius = 3f, int turns = 2)
    {
        var span = endPoint - startPoint;
        var distance = span.Length();
        var distPerPoint = distance / (turns * 4);
        var normal = span.Normalized();
        Curve3DPoint[] points = [CircleStartPoint(startPoint, radius * 2f, normal)];

        for (int i = 0; i < turns * 2; i++)
        {
            var circle = GetEllipse(
                points[^1].Position,
                points[^1].Out,
                normal,
                radius * 2f,
                radius * 2f
            );

            points = [.. points, .. circle.Skip(1)];
        }

        for (int i = 1; i < points.Length; i++)
        {
            // Progressively advance each point along the normal axis to create the spiral effect
            float advance = (distPerPoint * i) / (turns * 4);
            points[i].Position += normal * advance;
        }

        var curve = new Curve3D();
        points.ForEach(p => curve.AddPoint(p.Position, p.In, p.Out));

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

    /// <summary>
    /// Converts a center, diameter, and normal into a starting point (Position) and a starting tangent (Out).
    /// </summary>
    private static Curve3DPoint CircleStartPoint(Vector3 center, float diameter, Vector3 normal)
    {
        float radius = diameter / 2f;
        Vector3 N = normal.Normalized();

        // Fallback check for flat XZ horizontal planes
        Vector3 arbitraryAxis = Mathf.Abs(N.Dot(Vector3.Up)) < 0.99f ? Vector3.Up : Vector3.Forward;

        Vector3 localRight = N.Cross(arbitraryAxis).Normalized();

        Vector3 startPoint = center + (localRight * radius);
        Vector3 startPointTangent = localRight.Cross(N).Normalized();

        // Return the config packed cleanly inside your struct
        return new Curve3DPoint(startPoint, Vector3.Zero, startPointTangent);
    }

    /// <summary>
    /// Generates a 3-point Bezier segment representing a half-ellipse.
    /// </summary>
    private static Curve3DPoint[] GetSemiEllipse(Vector3 startPoint, Vector3 tangent, Vector3 normal, float width, float height)
    {
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        float handleW = halfWidth * Kappa;
        float handleH = halfHeight * Kappa;

        // Tangent magnitude should not affect geometry; only direction is meaningful here.
        Vector3 tangentDir = tangent.Normalized();

        // Vector pointing directly from startPoint toward the center
        Vector3 ToCenter = normal.Cross(tangentDir).Normalized();
        Vector3 center = startPoint + (ToCenter * halfHeight);

        // Node Positions
        Vector3 p0 = startPoint;
        Vector3 p1 = center + (tangentDir * halfWidth); // 90° Peak
        Vector3 p2 = center + (ToCenter * halfHeight); // 180° endpoint (opposite side of center)

        Curve3DPoint[] halfPoints =
        [
            // Node 0: Start (0°)
            new Curve3DPoint(p0, -handleW * tangentDir, handleW * tangentDir),
        
            // Node 1: Peak (90°) - Handles run parallel to the center axis, pointing away from each other
            new Curve3DPoint(p1, -handleH * ToCenter, handleH * ToCenter),
            
            // Node 2: End (180°) - Handles parallel to Tangent0, keeping the curve continuous
            new Curve3DPoint(p2, handleW * tangentDir, -handleW * tangentDir)
        ];

        return halfPoints;
    }

    private static Curve3DPoint[] GetEllipse(Vector3 startPoint, Vector3 startPointTangent, Vector3 normal, float width, float height)
    {
        // 1. Generate the first half (Nodes 0, 1, 2)
        Curve3DPoint[] firstHalf = GetSemiEllipse(startPoint, startPointTangent, normal, width, height);

        // 3. Generate the second half (Nodes 2, 3, 4) using the last element [^1]
        Curve3DPoint[] secondHalf = GetSemiEllipse(firstHalf[^1].Position, firstHalf[^1].Out, normal, width, height);

        // 4. Merge them seamlessly
        return [.. firstHalf, .. secondHalf.Skip(1)];
    }
}
