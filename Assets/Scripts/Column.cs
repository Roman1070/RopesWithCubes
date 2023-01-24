using RopeMinikit;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Column : MonoBehaviour
{
    [SerializeField] private ParticleSystem _frictionVfx;

    [SerializeField, ReadOnly] private bool _isTouchedByRope;
    [SerializeField, ReadOnly] private bool isPlaying;
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
        _isTouchedByRope = distance < 0.6f;
        _closestDistance = distance;
        if (_manager.IsMovingCubes && _isTouchedByRope)
        {
            if (!_frictionVfx.isPlaying)
            {
                _frictionVfx.Play();
            }
        }
        else
        {
            _frictionVfx.Stop();
        }


    }
}
