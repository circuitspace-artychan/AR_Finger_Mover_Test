using UnityEngine;

public class GrabbableObject : MonoBehaviour
{
    private Rigidbody rb;
    private bool isGrabbed = false;
    private Transform currentHand = null;   // the hand we�re attached to

    [Tooltip("Offset from the palm anchor when held (local space).")]
    public Vector3 holdOffset = new Vector3(0f, 0f, 0.07f); // tweak in Inspector later

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = true;
        rb.isKinematic = false;
    }

    private void Update()
    {
        // Manual release with Space
        if (isGrabbed && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space pressed: releasing" + name);
            DetachFromHand();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only grab if not already grabbed, and we hit something whose name contains "Hand"
        if (!isGrabbed && collision.transform.name.Contains("Hand"))
        {
            AttachToHand(collision.transform);
        }
    }

    // NOTE: no longer auto-release on collision exit; release is now via Space bar only.

    private void AttachToHand(Transform grabbedByHand)
    {
        if (rb == null || grabbedByHand == null)
            return;

        isGrabbed = true;
        currentHand = grabbedByHand;

        // Try to find the PalmAnchor child under the hand
        Transform palmAnchor = grabbedByHand.Find("PalmAnchor");

        // Disable physics while held
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Parent to palm (or to hand if no PalmAnchor)
        Transform parent = palmAnchor != null ? palmAnchor : grabbedByHand;
        transform.SetParent(parent);

        // Snap into place relative to palm
        transform.localPosition = holdOffset;
        transform.localRotation = Quaternion.identity;

        Debug.Log($"{name} grabbed by {grabbedByHand.name}");
    }

    private void DetachFromHand()
    {
        if (!isGrabbed)
            return;

        isGrabbed = false;

        // Unparent from hand
        transform.SetParent(null);
        currentHand = null;

        // Re-enable physics
        rb.isKinematic = false;
        rb.useGravity = true;

        Debug.Log($"{name} released");
    }
}
