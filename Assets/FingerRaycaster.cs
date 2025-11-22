using UnityEngine;

public class FingerRaycaster : MonoBehaviour
{
    public Transform thumbTip;
    public Transform indexTip;
    public Transform middleTip;
    public Transform ringTip;
    public Transform pinkyTip;

    public float rayLength = 2f;
    public LayerMask grabbableLayer;

    private Transform grabbedObject = null;
    private Rigidbody grabbedRb = null;
    private Collider grabbedCollider = null;

    void Update()
    {
        // Draw rays for debugging
        DebugDrawRay(thumbTip, Color.red);
        DebugDrawRay(indexTip, Color.green);
        DebugDrawRay(middleTip, Color.blue);
        DebugDrawRay(ringTip, Color.yellow);
        DebugDrawRay(pinkyTip, Color.magenta);

        // Toggle grab/release 
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (grabbedObject == null)
                TryGrab();
            else
                ReleaseObject();
        }

        // Smoothly move grabbed object toward fingertip
        if (grabbedObject != null)
        {
            Vector3 targetPos = indexTip.position;
            grabbedObject.position = Vector3.Lerp(grabbedObject.position, targetPos, Time.deltaTime * 10f); // Lerp "blends" the movement to finger so not such a big leap                                                                                                                                                                                                                                                  bbedObject.position, targetPos, Time.deltaTime * 10f); // Lerp "blends" the movement to the finger so not such a big leap
        }
    }

    void DebugDrawRay(Transform fingertip, Color color)
    {
        if (fingertip != null)
        {
            Vector3 direction = fingertip.forward; // adjust axis if needed
            Debug.DrawRay(fingertip.position, direction * rayLength, color, 0.1f, true);
        }
    }

    void TryGrab()
    {
        RaycastHit hit;
        Transform[] tips = { thumbTip, indexTip, middleTip, ringTip, pinkyTip };

        foreach (Transform tip in tips)
        {
            if (tip == null) continue;

            Vector3 direction = tip.forward; // or tip.forward if your hand is rotated differently
            if (Physics.Raycast(tip.position, direction, out hit, rayLength, grabbableLayer))
            {
                if (hit.collider != null)
                {
                    grabbedObject = hit.collider.transform;
                    grabbedRb = grabbedObject.GetComponent<Rigidbody>();
                    grabbedCollider = hit.collider;

                    // Disable physics and collisions while holding
                    if (grabbedRb != null)
                        grabbedRb.isKinematic = true;
                    if (grabbedCollider != null)
                        grabbedCollider.enabled = false;

                    Debug.Log("Grabbed: " + grabbedObject.name);
                    return; // grab only one object
                }
            }
        }
    }

    void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            // Re-enable collider and physics
            if (grabbedCollider != null)
                grabbedCollider.enabled = true;

            if (grabbedRb != null)
            {
                grabbedRb.isKinematic = false;
                grabbedRb = null;
            }

            Debug.Log("Released object");
            grabbedObject = null;
            grabbedCollider = null;
        }
    }
}
