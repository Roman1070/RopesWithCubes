using RopeMinikit;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Column : MonoBehaviour
{
    [SerializeField] private ParticleSystem _frictionVfx;

    [SerializeField, ReadOnly] private bool _isTouchedByRope;
    [SerializeField, ReadOnly] private float _closestDistance;
    private Rope _rope;
    private GameManager _manager;

    private void Start()
    {
        _rope = FindObjectOfType<Rope>();    
        _manager = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        _rope.GetClosestParticle(transform.position, out var index, out var distance);
        _closestDistance = distance;
        _isTouchedByRope = distance < 0.8f;

        if (_isTouchedByRope && !_frictionVfx.isPlaying && _manager.IsMovingCubes)
            _frictionVfx.Play();
        else _frictionVfx.Stop();
    }
}
