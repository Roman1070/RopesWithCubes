using DG.Tweening;
using Facebook.Unity;
using RopeMinikit;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Color32 _colorRed;
    [SerializeField] private Color32 _colorGreen;
    [SerializeField] private Color32 _colorYellow;
    [SerializeField] private Color32 _colorBlue;
    [SerializeField] private Rope _rope;
    [SerializeField] private Transform _ropeHead;
    [SerializeField] private Collider _planeCollider;
    [SerializeField] private InteractableCube _cubePrefab;
    [SerializeField, ReadOnly] private float _length;

    [Header("Animation Settings")]

    [SerializeField] private AnimationCurve _movementForwardAnimCurve;
    [SerializeField] private AnimationCurve _movementUprwardAnimCurve;
    [SerializeField] private float _movementForwardAnimDuration;
    [SerializeField] private float _movementUpwardAnimDuration;
    [SerializeField] private float _delayBeforeFalling;
    private InteractableCube _currentMainCube;
    private bool _isHolding;

    private float _timeSinceLastInteraction;

    private RopeConnection[] _connections;
    private RopeConnection _ropeHeadConnection;
    private RopeConnection _ropeTailConnection;
    private Tween _anchorMovementTween;


    public bool IsMovingCubes { get; private set; }

    void Awake()
    {
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }
    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    private void Start()
    {
        Application.targetFrameRate = 100;
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
                    switch (_currentMainCube.Value)
                    {
                        case 2:
                            _rope.material.color = _colorRed;
                            break;
                        case 4:
                            _rope.material.color = _colorGreen;
                            break;
                        case 8:
                            _rope.material.color = _colorYellow;
                            break;
                        case 16:
                            _rope.material.color = _colorBlue;
                            break;
                    }
                    cube.PlayBounceAnim();
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
                        _ropeHeadConnection.transformSettings.transform = cube.ModelsHolder;
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

        float delay = 0.0001f*length/30;
        var mainAnchor = _currentMainCube.RopeAttachmentPoint;

        _anchorMovementTween = DOVirtual.DelayedCall(1.5f* length/10f, () =>
        {
            //mainAnchor.DOMoveY(-4, 1.2f* length/13);
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

        if (dominant.Value == recessive.Value)
        {
            dominant.Rigidbody.isKinematic = true;
            recessive.Rigidbody.isKinematic = true;

            dominant.SmokeTrail.Stop();
            recessive.SmokeTrail.Stop();
            dominant.SmokeTrail.transform.parent = null;
            recessive.SmokeTrail.transform.parent = null;

            dominant.transform.DOMove(dominant.transform.position + dominant.transform.up*3 - dominant.transform.forward, _movementForwardAnimDuration).SetEase(_movementForwardAnimCurve);
            recessive.transform.DOMove(dominant.transform.position + dominant.transform.up*3 + dominant.transform.forward, _movementForwardAnimDuration).SetEase(_movementForwardAnimCurve);
            DOVirtual.DelayedCall(_movementForwardAnimDuration, ()=>
            {
                dominant.transform.DOMove(dominant.transform.position + dominant.transform.forward, _movementUpwardAnimDuration).SetEase(_movementUprwardAnimCurve);
                recessive.transform.DOMove(dominant.transform.position + dominant.transform.forward, _movementUpwardAnimDuration).SetEase(_movementUprwardAnimCurve);
                DOVirtual.DelayedCall(_movementUpwardAnimDuration, () =>
                 {
                     var newCube = Instantiate(_cubePrefab, dominant.transform.position, Quaternion.identity);
                     newCube.OnSpawned(recessive.Value * 2);
                     newCube.SmokeTrail.Stop();
                     newCube.Rigidbody.isKinematic = true;
                     Destroy(dominant.gameObject);
                     Destroy(recessive.gameObject);
                     DOVirtual.DelayedCall(_delayBeforeFalling, () =>
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
