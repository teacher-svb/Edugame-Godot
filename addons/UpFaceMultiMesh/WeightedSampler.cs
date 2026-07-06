#if TOOLS
using Godot;
using System.Collections.Generic;

public class WeightedSampler : ISampler
{
    private readonly List<(Vector3, Vector3, Vector3, float)> _faces;
    private readonly GeometryInstance3D _surface;
    private readonly float[] _cumulativeAreas;
    private readonly float _totalArea;

    public WeightedSampler(List<(Vector3, Vector3, Vector3, float)> faces, GeometryInstance3D surface)
    {
        _faces = faces;
        _surface = surface;
        _cumulativeAreas = new float[faces.Count];

        float cumulative = 0f;
        for (int i = 0; i < faces.Count; i++)
        {
            cumulative += faces[i].Item4;
            _cumulativeAreas[i] = cumulative;
        }
        _totalArea = cumulative;
    }

    public Vector3 Sample(RandomNumberGenerator rng)
    {
        float r = rng.Randf() * _totalArea;
        int faceIndex = 0;
        for (int i = 0; i < _cumulativeAreas.Length; i++)
        {
            if (r <= _cumulativeAreas[i])
            {
                faceIndex = i;
                break;
            }
        }

        var face = _faces[faceIndex];
        var point = RandomPointOnTriangle(face.Item1, face.Item2, face.Item3, rng);
        return _surface.GlobalTransform * point;
    }

    private Vector3 RandomPointOnTriangle(Vector3 a, Vector3 b, Vector3 c, RandomNumberGenerator rng)
    {
        float r1 = Mathf.Sqrt(rng.Randf());
        float r2 = rng.Randf();
        return a * (1 - r1) + b * (r1 * (1 - r2)) + c * (r1 * r2);
    }
}
#endif