using System.Collections.Generic;
using UnityEngine;

public class HandGrabManager : MonoBehaviour
{
    [Tooltip("How many fingers must touch an object before grabbing it?")]
    public int fingersNeededToGrab = 1;

    private Dictionary<Collider, HashSet<FingerTipCollider>> objectFingerMap = new();
    private GameObject grabbedObject = null;
    private Rigidbody handRb;

    void Start()
    {
        handRb = GetComponent<Rigidbody>();
        if (handRb == null)
        {
            handRb = gameObject.AddComponent<Rigidbody>();
            handRb.isKinematic = true; // the hand moves by script, not physics
        }
    }

    void Update()
    {
        // Manual release key (Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (grabbedObject != null)
                ReleaseObject();
        }
    }

    // Called by each fingertip when it touches something grabbable
    public void RegisterFingerTouch(FingerTipCollider finger, Collider other)
    {
        if (!other.CompareTag("Grabbable") || grabbedObject != null)
            return;

        if (!objectFingerMap.ContainsKey(other))
            objectFingerMap[other] = new HashSet<FingerTipCollider>();

        objectFingerMap[other].Add(finger);

        // If enough fingers are touching, grab the object
        if (objectFingerMap[other].Count >= fingersNeededToGrab)
        {
            GrabObject(other.attachedRigidbody);
        }
    }

    // Called when a fingertip stops touching
    public void UnregisterFingerTouch(FingerTipCollider finger, Collider other)
    {
        if (!objectFingerMap.ContainsKey(other))
            return;

        objectFingerMap[other].Remove(finger);

        if (objectFingerMap[other].Count == 0)
            objectFingerMap.Remove(other);

        // If we were holding this object and no fingers are touching, release it
        if (grabbedObject != null && other.gameObject == grabbedObject && objectFingerMap.Count == 0)
        {
            ReleaseObject();
        }
    }

    private void GrabObject(Rigidbody targetRb)
    {
        if (targetRb == null || handRb == null)
            return;

        grabbedObject = targetRb.gameObject;

        // Attach to palm instead of fingertip
        Transform palm = transform.Find("PalmAnchor");
        if (palm != null)
            grabbedObject.transform.SetParent(palm, true);

        targetRb.useGravity = false;
        targetRb.isKinematic = true;

        Debug.Log("Grabbed: " + grabbedObject.name);
    }

    private void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            grabbedObject.transform.SetParent(null);

            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            Debug.Log("Released: " + grabbedObject.name);
        }

        grabbedObject = null;
        objectFingerMap.Clear();
    }
}

