using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableCube : MonoBehaviour
{
    public Collider Collider;
    public Transform RopeAttachmentPoint;

    [Button]
    private void SetRefs()
    {
        Collider = GetComponent<Collider>();
        RopeAttachmentPoint = transform.GetChild(0);
    }
}
