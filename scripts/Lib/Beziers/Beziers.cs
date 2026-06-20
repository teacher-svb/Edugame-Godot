using Godot;
using System;
using System.Linq;
using TnT.Extensions;

/// <summary>
/// Factory helpers for generating common <see cref="Curve3D"/> shapes using cubic Bezier control points.
/// </summary>
public static class Beziers
{
    private const float Kappa = 0.5522847498f; // 4/3 * tan(π/8) — best 4-segment circle approximation

    // ─── Flat / 2-D shapes ────────────────────────────────────────────────────

    /// <summary>
    /// Creates a two-point cubic Bezier that represents a straight segment.
    /// </summary>
    /// <param name="startPoint">World-space position of the first anchor point.</param>
    /// <param name="endPoint">World-space position of the second anchor point.</param>
    /// <returns>
    /// A <see cref="Curve3D"/> containing two points with symmetric handles,
    /// producing linear interpolation between <paramref name="startPoint"/> and <paramref name="endPoint"/>.
    /// </returns>
    public static Curve3D Straight(Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 dir = (endPoint - startPoint) / 3f;
        var curve = new Curve3D();
        curve.AddPoint(startPoint, Vector3.Zero, dir);
        curve.AddPoint(endPoint, -dir, Vector3.Zero);
        return curve;
    }

    /// <summary>
    /// Creates a parabolic-like arch from start to end that peaks above the midpoint.
    /// </summary>
    /// <param name="startPoint">World-space position of the first anchor point.</param>
    /// <param name="endPoint">World-space position of the second anchor point.</param>
    /// <param name="height">Peak offset above the midpoint measured along <see cref="Vector3.Up"/>.</param>
    /// <returns>A two-point <see cref="Curve3D"/> configured as a smooth arch.</returns>
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

    // ─── Closed shapes ────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a closed circle in the plane defined by <paramref name="normal"/>.
    /// </summary>
    /// <param name="center">Circle center in world space.</param>
    /// <param name="radius">Circle radius.</param>
    /// <param name="normal">Normal vector of the circle plane.</param>
    /// <returns>A closed <see cref="Curve3D"/> approximating a circle with cubic segments.</returns>
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
    /// Creates a semicircle in the plane defined by <paramref name="normal"/>.
    /// </summary>
    /// <param name="center">Semicircle center in world space.</param>
    /// <param name="radius">Semicircle radius.</param>
    /// <param name="normal">Normal vector of the semicircle plane.</param>
    /// <returns>An open <see cref="Curve3D"/> representing half of an ellipse/circle.</returns>
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
    /// Creates a path that enters a vertical loop, traverses the loop, then exits.
    /// </summary>
    /// <param name="startPoint">Entry point before the loop.</param>
    /// <param name="endPoint">Exit point after the loop.</param>
    /// <param name="loopWidth">Loop diameter measured along the tangent direction.</param>
    /// <param name="loopHeight">Loop diameter measured along the loop normal axis.</param>
    /// <returns>A <see cref="Curve3D"/> composed of entry, loop, and exit control points.</returns>
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
    /// Creates a sine-like wave approximation between two points.
    /// </summary>
    /// <param name="startPoint">Wave start position.</param>
    /// <param name="endPoint">Wave end position.</param>
    /// <param name="normal">Normal used to orient the wave plane.</param>
    /// <param name="amplitude">Peak offset from the centerline.</param>
    /// <param name="cycles">Number of full wave cycles to fit across the span.</param>
    /// <returns>A <see cref="Curve3D"/> built from repeated semi-ellipse segments.</returns>
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
    /// Creates a constant-radius helix progressing from <paramref name="startPoint"/> to <paramref name="endPoint"/>.
    /// </summary>
    /// <param name="startPoint">Helix start position.</param>
    /// <param name="endPoint">Helix end position and axis direction reference.</param>
    /// <param name="radius">Radius of each turn.</param>
    /// <param name="turns">Number of full turns along the axis.</param>
    /// <returns>A <see cref="Curve3D"/> approximating a 3D spiral.</returns>
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
    /// Computes a deterministic start anchor and tangent for a circle/ellipse in a plane.
    /// </summary>
    /// <param name="center">Center of the shape.</param>
    /// <param name="diameter">Full diameter of the shape.</param>
    /// <param name="normal">Normal vector of the shape plane.</param>
    /// <returns>
    /// A <see cref="Curve3DPoint"/> where <c>Position</c> is the first anchor and <c>Out</c> is the initial tangent.
    /// </returns>
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
    /// Generates one half of an ellipse as three anchor points with bezier handles.
    /// </summary>
    /// <param name="startPoint">Start anchor of the half-ellipse.</param>
    /// <param name="tangent">Outgoing tangent direction at <paramref name="startPoint"/>.</param>
    /// <param name="normal">Plane normal used to orient the segment.</param>
    /// <param name="width">Full width of the ellipse.</param>
    /// <param name="height">Full height of the ellipse.</param>
    /// <returns>Three <see cref="Curve3DPoint"/> values describing the half segment.</returns>
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

    /// <summary>
    /// Generates a full ellipse by stitching two semi-ellipse segments.
    /// </summary>
    /// <param name="startPoint">Start anchor of the ellipse.</param>
    /// <param name="startPointTangent">Outgoing tangent direction at the start anchor.</param>
    /// <param name="normal">Plane normal used to orient the ellipse.</param>
    /// <param name="width">Full width of the ellipse.</param>
    /// <param name="height">Full height of the ellipse.</param>
    /// <returns>Five <see cref="Curve3DPoint"/> values describing the full ellipse.</returns>
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
