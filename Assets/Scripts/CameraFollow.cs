using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float smoothSpeed = 0.125f;

    private void LateUpdate()
    {
        if (target == null) return;

        // We only want to follow the Z axis (forward movement), 
        // and maybe keep X centered or follow X slightly?
        // Usually in runners, camera stays centered on X=0 or follows loosely.
        // Let's stick to simple offset for now.
        
        Vector3 desiredPosition = new Vector3(0, target.position.y, target.position.z) + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // We might want to fix X to 0 to keep the lane view steady
        smoothedPosition.x = 0; 
        
        transform.position = smoothedPosition;
    }
}
