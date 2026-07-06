using Godot;


namespace TnT.Extensions
{
    /// <summary>
    /// Extension methods for converting and snapping Godot <see cref="Vector2"/> and
    /// <see cref="Vector3"/> values. Conversion helpers make the mapping between axes
    /// explicit (XY vs XZ) to avoid ambiguity when switching between 2D and 3D coordinate spaces.
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        /// Converts a <see cref="Vector3"/> to a <see cref="Vector2"/> using the XZ plane
        /// (equivalent to <see cref="ToVector2XZ"/>). Use this when the Y axis is the vertical
        /// axis and X/Z represent the horizontal plane.
        /// </summary>
        /// <param name="v">The vector to convert.</param>
        /// <returns>A <see cref="Vector2"/> with components <c>(v.X, v.Z)</c>.</returns>
        public static Vector2 ToVector2(this Vector3 v) => ToVector2XZ(v);

        /// <summary>
        /// Converts a <see cref="Vector3"/> to a <see cref="Vector2"/> by dropping the Y component,
        /// mapping <c>X → X</c> and <c>Z → Y</c>.
        /// </summary>
        /// <param name="v">The vector to convert.</param>
        /// <returns>A <see cref="Vector2"/> with components <c>(v.X, v.Z)</c>.</returns>
        public static Vector2 ToVector2XZ(this Vector3 v) => new Vector2(v.X, v.Z);

        /// <summary>
        /// Converts a <see cref="Vector3"/> to a <see cref="Vector2"/> by dropping the Z component,
        /// mapping <c>X → X</c> and <c>Y → Y</c>.
        /// </summary>
        /// <param name="v">The vector to convert.</param>
        /// <returns>A <see cref="Vector2"/> with components <c>(v.X, v.Y)</c>.</returns>
        public static Vector2 ToVector2XY(this Vector3 v) => new Vector2(v.X, v.Y);

        /// <summary>
        /// Converts a <see cref="Vector2"/> to a <see cref="Vector3"/> using the XY plane
        /// (equivalent to <see cref="ToVector3XY"/>). Z is set to <c>0</c>.
        /// </summary>
        /// <param name="v">The vector to convert.</param>
        /// <returns>A <see cref="Vector3"/> with components <c>(v.X, v.Y, 0)</c>.</returns>
        public static Vector3 ToVector3(this Vector2 v) => ToVector3XY(v);

        /// <summary>
        /// Converts a <see cref="Vector2"/> to a <see cref="Vector3"/> on the XY plane,
        /// mapping <c>X → X</c> and <c>Y → Y</c>, with Z set to <c>0</c>.
        /// </summary>
        /// <param name="v">The vector to convert.</param>
        /// <returns>A <see cref="Vector3"/> with components <c>(v.X, v.Y, 0)</c>.</returns>
        public static Vector3 ToVector3XY(this Vector2 v) => new Vector3(v.X, v.Y, 0);

        /// <summary>
        /// Converts a <see cref="Vector2"/> to a <see cref="Vector3"/> on the XZ plane,
        /// mapping <c>X → X</c> and <c>Y → Z</c>, with Y set to <c>0</c>.
        /// Use this when the 2D vector represents a position on the horizontal ground plane.
        /// </summary>
        /// <param name="v">The vector to convert.</param>
        /// <returns>A <see cref="Vector3"/> with components <c>(v.X, 0, v.Y)</c>.</returns>
        public static Vector3 ToVector3XZ(this Vector2 v) => new Vector3(v.X, 0, v.Y);

        /// <summary>
        /// Snaps each component of <paramref name="v"/> to the nearest multiple of <paramref name="gridSize"/>.
        /// </summary>
        /// <param name="v">The position to snap.</param>
        /// <param name="gridSize">The grid cell size. Defaults to <c>16.0</c>.</param>
        /// <returns>The snapped <see cref="Vector3"/>.</returns>
        public static Vector3 Snap(this Vector3 v, float gridSize = 16.0f)
        {
            return new Vector3(
                Mathf.Round(v.X / gridSize) * gridSize,
                Mathf.Round(v.Y / gridSize) * gridSize,
                Mathf.Round(v.Z / gridSize) * gridSize);
        }
        /// <summary>
        /// Snaps each component of <paramref name="v"/> to the nearest multiple of <paramref name="gridSize"/>.
        /// </summary>
        /// <param name="v">The position to snap.</param>
        /// <param name="gridSize">The grid cell size. Defaults to <c>16.0</c>.</param>
        /// <returns>The snapped <see cref="Vector2"/>.</returns>
        public static Vector2 Snap(this Vector2 v, float gridSize = 16.0f)
        {
            return v.ToVector3().Snap(gridSize).ToVector2();
        }

        /// <summary>
        /// Snaps <paramref name="v"/> to the nearest grid position while accounting for an
        /// <paramref name="offset"/>. The offset is added before snapping and subtracted
        /// afterward, so the result is aligned to a grid that is shifted by
        /// <paramref name="offset"/> relative to the world origin.
        /// </summary>
        /// <param name="v">The position to snap.</param>
        /// <param name="offset">The grid origin offset.</param>
        /// <param name="gridSize">The grid cell size. Defaults to <c>16.0</c>.</param>
        /// <returns>The offset-snapped <see cref="Vector3"/>.</returns>
        public static Vector3 SnapOffset(this Vector3 v, Vector3 offset, float gridSize = 16.0f)
        {
            var shifted = v + offset;
            var snapped = shifted.Snap(gridSize);
            return snapped - offset;
        }

        /// <summary>
        /// Snaps <paramref name="v"/> to the nearest grid position while accounting for an
        /// <paramref name="offset"/>. The offset is added before snapping and subtracted
        /// afterward, so the result is aligned to a grid that is shifted by
        /// <paramref name="offset"/> relative to the world origin.
        /// </summary>
        /// <param name="v">The position to snap.</param>
        /// <param name="offset">The grid origin offset.</param>
        /// <param name="gridSize">The grid cell size. Defaults to <c>16.0</c>.</param>
        /// <returns>The offset-snapped <see cref="Vector2"/>.</returns>
        public static Vector2 SnapOffset(this Vector2 v, Vector2 offset, float gridSize = 16.0f)
        {
            return v.ToVector3().SnapOffset(offset.ToVector3(), gridSize).ToVector2();
        }
    }
}