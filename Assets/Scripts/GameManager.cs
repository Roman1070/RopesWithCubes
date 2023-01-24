using DG.Tweening;
using RopeMinikit;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Rope _rope;
    [SerializeField] private Transform _ropeHead;
    [SerializeField] private Collider _planeCollider;
    [SerializeField] private InteractableCube _cubePrefab;
    [SerializeField, ReadOnly] private float _length;
    [SerializeField] private AnimationCurve _firstAnimCurve;
    [SerializeField] private AnimationCurve _secondAnimCurve;
    private InteractableCube _currentMainCube;
    private bool _isHolding;

    private float _timeSinceLastInteraction;

    private RopeConnection[] _connections;
    private RopeConnection _ropeHeadConnection;
    private RopeConnection _ropeTailConnection;
    private Tween _anchorMovementTween;


    public bool IsMovingCubes;
    private void Start()
    {
        _rope.gameObject.SetActive(false);
        _connections = _rope.GetComponentsInChildren<RopeConnection>();
        _ropeHeadConnection = _connections[0];
        _ropeTailConnection = _connections[1];
    }

    private void Update()
    {
        _timeSinceLastInteraction += Time.deltaTime;
        _length = _rope.GetCurrentLength();
        if (Input.GetMouseButton(0) && _isHolding)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var pos = hit.point;
                _ropeHead.position = new Vector3(pos.x,-0.32f,pos.z);
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.collider.TryGetComponent<InteractableCube>(out var cube))
                {
                    _currentMainCube = cube;
                    _currentMainCube.RopeAttachmentPoint.transform.localPosition = Vector3.zero;
                    cube.IsMain = true;
                    //_currentMainCube.Collider.enabled = false;
                    _ropeTailConnection.transformSettings.transform = cube.RopeAttachmentPoint;
                    _ropeHeadConnection.transformSettings.transform = _ropeHead;
                    _isHolding = true;
                    _rope.gameObject.SetActive(true);
                    float radius = _rope.radius;
                    _rope.radius = 0;
                    DOVirtual.DelayedCall(0.12f, () =>
                    {
                        _rope.ResetToSpawnCurve();
                        _rope.radius = radius;
                    });
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (_currentMainCube != null)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit))
                {
                    if (hit.collider.TryGetComponent<InteractableCube>(out var cube) && cube.gameObject != _currentMainCube.gameObject)
                    {
                        _ropeHeadConnection.transformSettings.transform = cube.transform;
                        StartCoroutine(MoveCubeAlongTheRope(cube.transform));
                    }
                    else
                    {
                        _rope.gameObject.SetActive(false);
                        _currentMainCube.Collider.enabled = true;
                        _currentMainCube.IsMain = false;
                        _currentMainCube = null;
                    }
                }
                else
                {
                    _rope.gameObject.SetActive(false);
                    _currentMainCube.Collider.enabled = true;
                    _currentMainCube.IsMain = false;
                    _currentMainCube = null;
                }

                _isHolding = false;
            }
        }

    }
    private IEnumerator MoveCubeAlongTheRope(Transform cube)
    {
        IsMovingCubes = true;
        float length = _length;
        List<Vector3> positions = new List<Vector3>();
        for (int i = _rope.measurements.particleCount - 1; i >= 0; i--)
        {
            var pos = new Vector3(_rope.GetPositionAt(i).x, _currentMainCube.transform.position.y, _rope.GetPositionAt(i).z);
            positions.Add(pos);
        }

        float delay = 0.0025f*length/10;
        var mainAnchor = _currentMainCube.RopeAttachmentPoint;

        _anchorMovementTween = DOVirtual.DelayedCall(0.4f* length/10f, () =>
        {
            mainAnchor.DOMoveY(-4, 1.2f* length/13);
        });
        for (int i =0; i <= _rope.measurements.particleCount - 1; i++)
        {
            cube.DOMove(positions[i], delay);
            yield return new WaitForSeconds(delay);
        }
        IsMovingCubes = false;
    }


    public void OnCubesIntersected(InteractableCube dominant, InteractableCube recessive)
    {
        if (_timeSinceLastInteraction <= 1) return;
        IsMovingCubes = true;
        float firstAnimDuration = 0.35f;
        float secondAnimDuration = 0.3f;
        if (dominant.Value == recessive.Value)
        {
            dominant.Rigidbody.isKinematic = true;
            recessive.Rigidbody.isKinematic = true;
            
            dominant.transform.DOMove(dominant.transform.position + dominant.transform.up*3 - dominant.transform.forward, firstAnimDuration).SetEase(_firstAnimCurve);
            recessive.transform.DOMove(dominant.transform.position + dominant.transform.up*3 + dominant.transform.forward, firstAnimDuration).SetEase(_firstAnimCurve);
            DOVirtual.DelayedCall(firstAnimDuration, ()=>
            {
                dominant.transform.DOMove(dominant.transform.position + dominant.transform.forward, secondAnimDuration).SetEase(_secondAnimCurve);
                recessive.transform.DOMove(dominant.transform.position + dominant.transform.forward, secondAnimDuration).SetEase(_secondAnimCurve);
                DOVirtual.DelayedCall(secondAnimDuration, () =>
                 {
                     var newCube = Instantiate(_cubePrefab, dominant.transform.position, Quaternion.identity);
                     newCube.OnSpawned(recessive.Value * 2);
                     newCube.Rigidbody.isKinematic = true;
                     Destroy(dominant.gameObject);
                     Destroy(recessive.gameObject);
                     DOVirtual.DelayedCall(0.25f, () =>
                     {
                         newCube.Rigidbody.isKinematic = false;
                     });
                 });
                
            });
           
        }
        
        DOVirtual.DelayedCall(0.1f,()=>
        {
            _anchorMovementTween.Kill();
            StopAllCoroutines();
        });

        _currentMainCube.IsMain = false;
        _currentMainCube = null;
        IsMovingCubes = false;

        _rope.gameObject.SetActive(false);
        _rope.collisions.enabled = true;
        _timeSinceLastInteraction = 0;
    }
}
