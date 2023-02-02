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
    public Transform ModelsHolder;
    public Transform RopeAttachmentPoint;
    public Rigidbody Rigidbody;
    public ParticleSystem SmokeTrail;
    public ParticleSystem OnDestroyVFX;

    public bool IsMain;
    public float Value;
    [SerializeField] private float _startLocalScale;
    [SerializeField] private MeshRenderer[] _fracturedParts;
    [SerializeField] private Rigidbody[] _fracturedPartsRbs;
    [SerializeField] private Collider[] _fracturedPartsColliders;
    [SerializeField] private AnimationCurve _parabola;
    [SerializeField] private Material[] _colors;
    [SerializeField] private GameObject _unbrokenModel;
    [SerializeField] private GameObject _fracturedModel;

    private void Awake()
    {
        _fracturedPartsColliders.ForEach(c => c.enabled = false);
    }

    public void OnSpawned(float value)
    {
        Value = value;
        foreach(var part in _fracturedParts)
        {
            part.material = _colors[Convert.ToInt32(Mathf.Sqrt(value)) - 1];
        }
        transform.localScale = Vector3.one * _startLocalScale;
        GetComponent<Animator>().SetTrigger("Appearence");
        //AppearenceEffect.Play();
        DOVirtual.DelayedCall(1, () =>
        {
            SmokeTrail.Play();
        });
    }

    public void Destroy()
    {
        _unbrokenModel.SetActive(false);
        _fracturedModel.SetActive(true);
        OnDestroyVFX.Play();
        foreach (var part in _fracturedPartsRbs)
        {
            //_fracturedPartsColliders.ForEach(c => c.enabled = true);
            part.isKinematic = false;
            part.useGravity = false;
            part.AddExplosionForce(8, transform.position-Vector3.forward, 3,1,ForceMode.Impulse);
            DOVirtual.Float(0, 1, 3, t =>
            {
                part.AddForce(new Vector3(0, 0, -16f), ForceMode.Force);
            }).SetUpdate(UpdateType.Fixed);
        }
        GetComponent<Collider>().enabled = false;
        Destroy(this);
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
        if (collision.collider.TryGetComponent<InteractableCube>(out var cube) && IsMain)
        {
            FindObjectOfType<GameManager>().OnCubesIntersected(this, cube);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
       
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.parent.TryGetComponent<Column>(out var column))
        {
            Vector3 columnToCubeVector = (transform.position - column.transform.position).normalized;
            ModelsHolder.transform.DOLocalMove( columnToCubeVector * transform.localScale.x*1.2f,0.05f);
        }
    }
}
