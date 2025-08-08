using Godot;


namespace TnT.Extensions
{
    public static class VectorExtensions
    {
        public static Vector2 ToVector2(this Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }
        public static Vector3 ToVector3(this Vector2 v)
        {
            return new Vector3(v.X, v.Y, 0);
        }
        /// <summary>
        /// Snap Vector3 to nearest grid position
        /// </summary>
        /// <param name="v">Sloppy position</param>
        /// <param name="gridSize">Grid size</param>
        /// <returns>Snapped position</returns>
        public static Vector3 Snap(this Vector3 v, float gridSize = 16.0f)
        {
            return new Vector3(
                Mathf.Round(v.X / gridSize) * gridSize,
                Mathf.Round(v.Y / gridSize) * gridSize,
                Mathf.Round(v.Z / gridSize) * gridSize);
        }
        public static Vector2 Snap(this Vector2 v, float gridSize = 16.0f)
        {
            return v.ToVector3().Snap(gridSize).ToVector2();
        }

        /// <summary>
        /// Snap Vector3 to nearest grid position with offset
        /// </summary>
        /// <param name="v">Sloppy position</param>
        /// <param name="gridSize">Grid size</param>
        /// <returns>Snapped position</returns>
        public static Vector3 SnapOffset(this Vector3 v, Vector3 offset, float gridSize = 16.0f)
        {
            Vector3 snapped = v + offset;
            snapped = new Vector3(
                Mathf.Round(snapped.X / gridSize) * gridSize,
                Mathf.Round(snapped.Y / gridSize) * gridSize,
                Mathf.Round(snapped.Z / gridSize) * gridSize);
            return snapped - offset;
        }
        public static Vector2 SnapOffset(this Vector2 v, Vector2 offset, float gridSize = 16.0f)
        {
            return v.ToVector3().SnapOffset(offset.ToVector3(), gridSize).ToVector2();
        }
    }
}