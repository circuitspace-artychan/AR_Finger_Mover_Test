using UnityEngine;

public class GrabbableObject : MonoBehaviour
{
    private Rigidbody rb;
    private FixedJoint joint;
    private bool isGrabbed = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only respond if the colliding object has a "WhiteHand" (or similar)
        if (!isGrabbed && collision.transform.name.Contains("Hand"))
        {
            AttachToHand(collision.transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Release when the hand stops touching the object
        if (isGrabbed && collision.transform.name.Contains("Hand"))
        {
            DetachFromHand();
        }
    }

    private void AttachToHand(Transform grabbedByHand)
    {
        if (rb == null || grabbedByHand == null)
            return;

        isGrabbed = true;
        rb.useGravity = false;
        rb.isKinematic = false; // joint will handle motion

        // Add a FixedJoint to connect the object to the hand (or palm)
        joint = gameObject.AddComponent<FixedJoint>();

        // Try to find the PalmAnchor child under the hand
        Transform palmAnchor = grabbedByHand.Find("PalmAnchor");

        if (palmAnchor != null && palmAnchor.GetComponent<Rigidbody>() != null)
        {
            // Connect to the PalmAnchor’s Rigidbody
            joint.connectedBody = palmAnchor.GetComponent<Rigidbody>();
        }
        else
        {
            // Fallback to the hand’s Rigidbody
            joint.connectedBody = grabbedByHand.GetComponent<Rigidbody>();
        }

        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;

        Debug.Log($"{name} grabbed by {grabbedByHand.name}");
    }

    private void DetachFromHand()
    {
        if (!isGrabbed)
            return;

        isGrabbed = false;
        rb.useGravity = true;

        // Remove the joint safely
        if (joint != null)
        {
            Destroy(joint);
            joint = null;
        }

        Debug.Log($"{name} released");
    }
}
