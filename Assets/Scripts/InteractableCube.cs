using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableCube : MonoBehaviour
{
    public Collider Collider;
    public Transform RopeAttachmentPoint;
    public MeshRenderer MeshRenderer;
    public Rigidbody Rigidbody;

    public bool IsMain;
    public float Value;

    public void OnSpawned()
    {
        Material newMaterial = new Material(Shader.Find("Standard"));
        MeshRenderer.material = newMaterial;
        newMaterial.color = Random.ColorHSV();
        transform.localScale = Vector3.one * 0.6f;
        GetComponent<Animator>().SetTrigger("Appearence");
    }

    [Button]
    private void SetRefs()
    {
        Collider = GetComponent<Collider>();
        RopeAttachmentPoint = transform.GetChild(0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.TryGetComponent<InteractableCube>(out var cube))
        {
            FindObjectOfType<GameManager>().OnCubesIntersected(IsMain?this:cube, IsMain? cube:this);
        }
    }
}
