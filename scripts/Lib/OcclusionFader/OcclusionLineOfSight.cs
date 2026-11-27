using System;
using System.Collections.Generic;
using Godot;

namespace TnT.Systems.OcclusionFade
{
    /// <summary>
    /// Is the line from the camera to the player blocked by geometry the <see cref="OcclusionFader"/> converted?
    /// Tested against the meshes themselves (no colliders needed): world bounds first, then a triangle BVH per mesh
    /// (built once per mesh, when its node is added). MeshInstance3D placement follows the node every frame (moving
    /// doors, a drawbridge); MultiMesh instance placement is cached until the MultiMeshInstance3D moves; GridMap cells
    /// until the GridMap moves, gets another MeshLibrary (<c>Changed</c>), or its set of used cells changes (checked about
    /// once a second; <c>Changed</c> doesn't fire for cell edits). An item swapped in place on a used cell is not seen.
    /// </summary>
    internal sealed class OcclusionLineOfSight
    {
        readonly List<MeshInstance3D> _meshes = new();
        readonly Dictionary<MultiMeshInstance3D, MultiMeshCache> _multiMeshes = new();
        readonly Dictionary<GridMap, GridMapCache> _gridMaps = new();
        readonly Dictionary<Mesh, TriangleBvh> _bvhs = new();

        // a mesh placed in the world: its world bounds, the transform from world to its local space, its triangles
        readonly record struct Item(Aabb Bounds, Transform3D ToLocal, TriangleBvh Bvh);

        sealed class MultiMeshCache
        {
            public Transform3D At;
            public MultiMesh Source;
            public List<Item> Items; // null: not built yet
        }

        sealed class GridMapCache
        {
            public List<Item> Items;   // null: not built yet, or the library changed
            public Transform3D At;     // the GridMap's transform the items were built for
            public int CellsHash;      // the used cells the items were built from
            public ulong NextCheckMs;  // when to compare the used cells again
        }

        const ulong CellCheckIntervalMs = 1000;

        public void Add(MeshInstance3D mesh)
        {
            _meshes.Add(mesh);
            if (mesh.Mesh != null) Bvh(mesh.Mesh); // built now (level load), not on the first test
        }

        public void Add(MultiMeshInstance3D multiMesh)
        {
            var cache = new MultiMeshCache();
            if (!_multiMeshes.TryAdd(multiMesh, cache)) return;
            if (multiMesh.IsInsideTree()) MultiMeshItems(multiMesh, cache); // placement now (level load)
        }

        public void Add(GridMap gridMap)
        {
            if (_gridMaps.ContainsKey(gridMap)) return;
            var cache = new GridMapCache();
            _gridMaps[gridMap] = cache;
            gridMap.Changed += () => cache.Items = null; // another MeshLibrary: rebuild on the next test
            if (gridMap.IsInsideTree()) GridMapItems(gridMap, cache); // cells now (level load)
        }

        TriangleBvh Bvh(Mesh mesh)
        {
            if (!_bvhs.TryGetValue(mesh, out var bvh))
                _bvhs[mesh] = bvh = new TriangleBvh(mesh.GetFaces());
            return bvh;
        }

        /// <summary>
        /// True when the segment from <paramref name="from"/> to <paramref name="to"/> crosses a triangle of a
        /// registered, visible node at a point for which <paramref name="counts"/> is true.
        /// </summary>
        public bool IsBlocked(Vector3 from, Vector3 to, Func<Vector3, bool> counts)
        {
            for (int i = _meshes.Count - 1; i >= 0; i--)
            {
                var mesh = _meshes[i];
                if (!GodotObject.IsInstanceValid(mesh)) { _meshes.RemoveAt(i); continue; }
                if (mesh.Mesh == null || !mesh.IsVisibleInTree()) continue;
                var bvh = Bvh(mesh.Mesh);
                if (bvh.IsEmpty) continue;
                var xf = mesh.GlobalTransform;
                if (Hits(new Item(xf * bvh.Bounds, xf.AffineInverse(), bvh), from, to, counts))
                    return true;
            }

            List<MultiMeshInstance3D> gone = null;
            foreach (var (instance, cache) in _multiMeshes)
            {
                if (!GodotObject.IsInstanceValid(instance)) { (gone ??= new()).Add(instance); continue; }
                if (!instance.IsVisibleInTree()) continue;
                foreach (var item in MultiMeshItems(instance, cache))
                    if (Hits(item, from, to, counts))
                        return true;
            }
            if (gone != null) foreach (var g in gone) _multiMeshes.Remove(g);

            List<GridMap> goneMaps = null;
            foreach (var (gridMap, cache) in _gridMaps)
            {
                if (!GodotObject.IsInstanceValid(gridMap)) { (goneMaps ??= new()).Add(gridMap); continue; }
                if (!gridMap.IsVisibleInTree()) continue;
                foreach (var item in GridMapItems(gridMap, cache))
                    if (Hits(item, from, to, counts))
                        return true;
            }
            if (goneMaps != null) foreach (var g in goneMaps) _gridMaps.Remove(g);
            return false;
        }

        List<Item> MultiMeshItems(MultiMeshInstance3D instance, MultiMeshCache cache)
        {
            var xf = instance.GlobalTransform;
            var multimesh = instance.Multimesh;
            if (cache.Items != null && cache.At == xf && cache.Source == multimesh)
                return cache.Items;

            var items = new List<Item>();
            var mesh = multimesh?.Mesh;
            if (mesh != null && multimesh.TransformFormat == MultiMesh.TransformFormatEnum.Transform3D)
            {
                var bvh = Bvh(mesh);
                int count = multimesh.VisibleInstanceCount >= 0 ? multimesh.VisibleInstanceCount : multimesh.InstanceCount;
                if (!bvh.IsEmpty)
                    for (int i = 0; i < count; i++)
                    {
                        var world = xf * multimesh.GetInstanceTransform(i);
                        items.Add(new Item(world * bvh.Bounds, world.AffineInverse(), bvh));
                    }
            }
            cache.At = xf;
            cache.Source = multimesh;
            cache.Items = items;
            return items;
        }

        // The cached cells, rebuilt when the GridMap moved, its library changed, or its used cells changed.
        List<Item> GridMapItems(GridMap gridMap, GridMapCache cache)
        {
            var xf = gridMap.GlobalTransform;
            ulong now = Time.GetTicksMsec();
            if (cache.Items != null && cache.At == xf)
            {
                if (now < cache.NextCheckMs)
                    return cache.Items;
                cache.NextCheckMs = now + CellCheckIntervalMs;
                if (CellsHash(gridMap.GetUsedCells()) == cache.CellsHash)
                    return cache.Items;
            }

            var items = new List<Item>();
            var cells = gridMap.GetUsedCells();
            var library = gridMap.MeshLibrary;
            if (library != null)
            {
                var scale = Vector3.One * gridMap.CellScale;
                foreach (Vector3I cell in cells)
                {
                    int id = gridMap.GetCellItem(cell);
                    var mesh = library.GetItemMesh(id);
                    if (mesh == null) continue;
                    var bvh = Bvh(mesh);
                    if (bvh.IsEmpty) continue;
                    // as GridMap places it: the cell's orientation, scaled by CellScale, at the cell's centre
                    var cellXf = new Transform3D(gridMap.GetCellItemBasis(cell).Scaled(scale), gridMap.MapToLocal(cell));
                    var world = xf * cellXf * library.GetItemMeshTransform(id);
                    items.Add(new Item(world * bvh.Bounds, world.AffineInverse(), bvh));
                }
            }
            cache.Items = items;
            cache.At = xf;
            cache.CellsHash = CellsHash(cells);
            cache.NextCheckMs = now + CellCheckIntervalMs;
            return items;
        }

        static int CellsHash(Godot.Collections.Array<Vector3I> cells)
        {
            var hash = new HashCode();
            hash.Add(cells.Count);
            foreach (var cell in cells) hash.Add(cell);
            return hash.ToHashCode();
        }

        static bool Hits(in Item item, Vector3 from, Vector3 to, Func<Vector3, bool> counts)
        {
            if (!TriangleBvh.SegmentHitsBox(from, to, item.Bounds.Position, item.Bounds.End))
                return false;
            return item.Bvh.Hits(item.ToLocal * from, item.ToLocal * to, item.ToLocal.AffineInverse(), counts);
        }
    }

    /// <summary>A mesh's triangles (local space) in a bounding volume hierarchy, for segment tests.</summary>
    internal sealed class TriangleBvh
    {
        const int LeafSize = 8;

        struct BvhNode
        {
            public Vector3 Min, Max;
            public int Left, Right;   // children (inner node)
            public int Start, Count;  // triangle range in _order (leaf: Count > 0)
        }

        readonly Vector3[] _faces;     // 3 vertices per triangle (Mesh.GetFaces)
        readonly int[] _order;         // triangle indices, grouped by leaf
        readonly List<BvhNode> _nodes = new();

        public bool IsEmpty => _order.Length == 0;
        public Aabb Bounds { get; }

        public TriangleBvh(Vector3[] faces)
        {
            _faces = faces ?? Array.Empty<Vector3>();
            int count = _faces.Length / 3;
            _order = new int[count];
            for (int i = 0; i < count; i++) _order[i] = i;
            if (count == 0) return;
            var centroids = new Vector3[count];
            for (int i = 0; i < count; i++)
                centroids[i] = (_faces[3 * i] + _faces[3 * i + 1] + _faces[3 * i + 2]) / 3f;
            Build(0, count, centroids);
            Bounds = new Aabb(_nodes[0].Min, _nodes[0].Max - _nodes[0].Min);
        }

        int Build(int start, int count, Vector3[] centroids)
        {
            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = -min;
            var cMin = min;
            var cMax = max;
            for (int i = start; i < start + count; i++)
            {
                int t = _order[i];
                for (int k = 0; k < 3; k++)
                {
                    var v = _faces[3 * t + k];
                    min = min.Min(v);
                    max = max.Max(v);
                }
                cMin = cMin.Min(centroids[t]);
                cMax = cMax.Max(centroids[t]);
            }

            int index = _nodes.Count;
            _nodes.Add(new BvhNode { Min = min, Max = max, Start = start, Count = count });
            if (count <= LeafSize)
                return index;

            var extent = cMax - cMin;
            int axis = extent.X >= extent.Y && extent.X >= extent.Z ? 0 : extent.Y >= extent.Z ? 1 : 2;
            Array.Sort(_order, start, count, Comparer<int>.Create((p, q) => centroids[p][axis].CompareTo(centroids[q][axis])));
            int half = count / 2;
            int left = Build(start, half, centroids);
            int right = Build(start + half, count - half, centroids);
            _nodes[index] = new BvhNode { Min = min, Max = max, Left = left, Right = right };
            return index;
        }

        /// <summary>Segment a→b (local space) against the triangles; `counts` gets the hit in world space.</summary>
        public bool Hits(Vector3 a, Vector3 b, Transform3D toWorld, Func<Vector3, bool> counts)
        {
            if (IsEmpty) return false;
            Span<int> stack = stackalloc int[64];
            int top = 0;
            stack[top++] = 0;
            while (top > 0)
            {
                var node = _nodes[stack[--top]];
                if (!SegmentHitsBox(a, b, node.Min, node.Max))
                    continue;
                if (node.Count > 0)
                {
                    for (int i = node.Start; i < node.Start + node.Count; i++)
                    {
                        int t = _order[i];
                        if (SegmentHitsTriangle(a, b, _faces[3 * t], _faces[3 * t + 1], _faces[3 * t + 2], out var hit)
                            && counts(toWorld * hit))
                            return true;
                    }
                }
                else if (top + 2 <= stack.Length)
                {
                    stack[top++] = node.Left;
                    stack[top++] = node.Right;
                }
            }
            return false;
        }

        public static bool SegmentHitsBox(Vector3 a, Vector3 b, Vector3 min, Vector3 max)
        {
            var d = b - a;
            float tMin = 0f, tMax = 1f;
            for (int axis = 0; axis < 3; axis++)
            {
                float origin = a[axis], dir = d[axis];
                float lo = min[axis], hi = max[axis];
                if (Mathf.Abs(dir) < 1e-8f)
                {
                    if (origin < lo || origin > hi) return false;
                    continue;
                }
                float t1 = (lo - origin) / dir, t2 = (hi - origin) / dir;
                if (t1 > t2) (t1, t2) = (t2, t1);
                tMin = Mathf.Max(tMin, t1);
                tMax = Mathf.Min(tMax, t2);
                if (tMin > tMax) return false;
            }
            return true;
        }

        // Möller–Trumbore, both sides, hit within the segment
        static bool SegmentHitsTriangle(Vector3 a, Vector3 b, Vector3 v0, Vector3 v1, Vector3 v2, out Vector3 hit)
        {
            hit = default;
            var dir = b - a;
            var e1 = v1 - v0;
            var e2 = v2 - v0;
            var p = dir.Cross(e2);
            float det = e1.Dot(p);
            if (Mathf.Abs(det) < 1e-12f) return false;
            float inv = 1f / det;
            var s = a - v0;
            float u = s.Dot(p) * inv;
            if (u < 0f || u > 1f) return false;
            var q = s.Cross(e1);
            float v = dir.Dot(q) * inv;
            if (v < 0f || u + v > 1f) return false;
            float t = e2.Dot(q) * inv;
            if (t < 0f || t > 1f) return false;
            hit = a + dir * t;
            return true;
        }
    }
}
