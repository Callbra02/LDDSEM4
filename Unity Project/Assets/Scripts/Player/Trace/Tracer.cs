using UnityEngine;

public class Tracer
{

    public static Trace TraceCollider(Collider collider, Vector3 origin, Vector3 end, int layerMask,
        float colliderScale = 1.0f)
    {
        if (collider is BoxCollider)
        {
            return TraceBox(origin, end, collider.bounds.extents, collider.contactOffset, layerMask, colliderScale);
        }

        throw new System.NotImplementedException("Trace missing for collider: " + collider.GetType());
    }

    public static Trace TraceBox(Vector3 start, Vector3 destination, Vector3 extents, float contactOffset,
        int layerMask, float colliderScale = 1.0f)
    {
        var result = new Trace()
        {
            startPosition = start,
            endPosition = destination
        };

        var longSide = Mathf.Sqrt(contactOffset * contactOffset + contactOffset * contactOffset);
        var dir = (destination - start).normalized;
        var maxDist = Vector3.Distance(start, destination) + longSide;
        extents *= (1.0f - contactOffset);

        RaycastHit hit;
        if (Physics.BoxCast(center: start,
                halfExtents: extents * colliderScale,
                direction: dir,
                orientation: Quaternion.identity,
                maxDistance: maxDist,
                hitInfo: out hit,
                layerMask: layerMask,
                queryTriggerInteraction: QueryTriggerInteraction.Ignore))
        {
            result.fraction = hit.distance / maxDist;
            result.hitCollider = hit.collider;
            result.hitPoint = hit.point;
            result.planeNormal = hit.normal;
            result.distance = hit.distance;

            RaycastHit normalHit;
            Ray normalRay = new Ray(hit.point - dir * 0.001f, dir);
            if (hit.collider.Raycast(normalRay, out normalHit, 0.002f))
            {
                result.planeNormal = normalHit.normal;
            }

        }
        else
        {
            result.fraction = 1;
        }

        return result;
    }
}
