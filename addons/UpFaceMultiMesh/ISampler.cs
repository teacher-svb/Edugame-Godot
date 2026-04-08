#if TOOLS
using Godot;

public interface ISampler
{
    Vector3 Sample(RandomNumberGenerator rng);
}
#endif