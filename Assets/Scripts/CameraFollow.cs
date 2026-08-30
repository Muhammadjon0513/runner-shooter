using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float smoothTime = 0.15f; // BUG-4 Fix: smoothSpeed o'rniga smoothTime

    // BUG-4 Fix: SmoothDamp uchun velocity
    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        // We only want to follow the Z axis (forward movement), 
        // and maybe keep X centered or follow X slightly?
        // Usually in runners, camera stays centered on X=0 or follows loosely.
        // Let's stick to simple offset for now.
        
        Vector3 desiredPosition = new Vector3(0, target.position.y, target.position.z) + offset;
        
        // BUG-4 Fix: SmoothDamp — FPS-ga bog'liq emas
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        
        // We might want to fix X to 0 to keep the lane view steady
        smoothedPosition.x = 0; 
        
        transform.position = smoothedPosition;
    }
}
