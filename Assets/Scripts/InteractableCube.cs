using DG.Tweening;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableCube : MonoBehaviour
{
    public Collider Collider;
    public Transform RopeAttachmentPoint;
    public MeshRenderer MeshRenderer;
    public Rigidbody Rigidbody;
    public ParticleSystem SmokeTrail;

    public bool IsMain;
    public float Value;
    [SerializeField] private GameObject[] _models;

    public void OnSpawned(float value)
    {
        Value = value;
        _models.ForEach(m => m.SetActive(false));
        _models[Convert.ToInt32(Mathf.Sqrt(value)) - 1].SetActive(true);
        transform.localScale = Vector3.one * 0.35f;
        GetComponent<Animator>().SetTrigger("Appearence");

        DOVirtual.DelayedCall(1, () =>
        {
            SmokeTrail.Play();
        });
    }

    public void PlayBounceAnim()
    {
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
