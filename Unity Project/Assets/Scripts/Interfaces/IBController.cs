using UnityEngine;

public interface IBController
{
    MovementData movementData { get; }
    Collider collider { get; }
    GameObject groundObject { get; set; }
    Vector3 forward { get; }
    Vector3 right { get; }
    Vector3 up { get; }
    Vector3 baseVelocity { get; }
}
