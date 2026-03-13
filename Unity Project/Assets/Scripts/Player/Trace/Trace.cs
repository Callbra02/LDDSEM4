using UnityEngine;

public struct Trace
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public float fraction;
    public bool startSolid;
    public Collider hitCollider;
    public Vector3 hitPoint;
    public Vector3 planeNormal;
    public float distance;
}
