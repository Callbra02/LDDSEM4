using UnityEngine;

public class BPhysics
{
    public static int groundLayerMask = LayerMask.GetMask(new string[] { "Default", "Ground" });

    private static Collider[] _colliders = new Collider[_maxCollisions];
    private static Vector3[] _planes = new Vector3[_maxClipPlanes];

    public const float HU2M = 52.4934383202f;
    private const int _maxCollisions = 128;
    private const int _maxClipPlanes = 5;
    private const int _numberOfBumps = 1;

    public static void ResolveCollisions(Collider collider, ref Vector3 origin, ref Vector3 velocity,
        float rigidbodyPushForce, float velocityMultiplier = 1.0f, float stepOffset = 0.0f,
        IBController controller = null)
    {
        
    }

}
