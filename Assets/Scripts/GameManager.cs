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
    private InteractableCube _currentMainCube;
    private bool _isHolding;

    private RopeConnection[] _connections;

    private RopeConnection _ropeHeadConnection;
    private RopeConnection _ropeTailConnection;


    private void Start()
    {
        _rope.gameObject.SetActive(false);
        _connections = _rope.GetComponentsInChildren<RopeConnection>();
        _ropeHeadConnection = _connections[0];
        _ropeTailConnection = _connections[1];
    }

    private void Update()
    {
        _length = _rope.GetCurrentLength();
        if (Input.GetMouseButton(0) && _isHolding)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var pos = hit.point;
                _ropeHead.position = pos;
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
                    _currentMainCube.Collider.enabled = false;
                    _ropeTailConnection.transformSettings.transform = cube.RopeAttachmentPoint;
                    _ropeHeadConnection.transformSettings.transform = _ropeHead;
                    _isHolding = true;
                    _rope.gameObject.SetActive(true);
                    float radius = _rope.radius;
                    _rope.radius = 0;
                    DOVirtual.DelayedCall(0.1f, () =>
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
                        _currentMainCube = null;
                    }
                }
                else
                {
                    _rope.gameObject.SetActive(false);
                    _currentMainCube.Collider.enabled = true;
                    _currentMainCube = null;
                }

                _isHolding = false;
            }
        }

    }
    private IEnumerator MoveCubeAlongTheRope(Transform cube)
    {
        float length = _length;
        List<Vector3> positions = new List<Vector3>();
        for (int i = _rope.measurements.particleCount - 1; i >= 0; i--)
        {
            var pos = new Vector3(_rope.GetPositionAt(i).x, _currentMainCube.transform.position.y, _rope.GetPositionAt(i).z);
            positions.Add(pos);
        }

        float delay = 0.025f/ length;
        var mainAnchor = _currentMainCube.RopeAttachmentPoint;

        _rope.collisions.enabled = false;

        DOVirtual.DelayedCall(0.5f* length/10f, () =>
        {
            mainAnchor.DOMoveY(-4, 1.2f* length/13);
        });
        for (int i =0; i <= _rope.measurements.particleCount - 1; i++)
        {
            cube.DOMove(positions[i], delay);
            yield return new WaitForSeconds(delay);
        }
        var newCube = Instantiate(_cubePrefab, positions[positions.Count-1], Quaternion.identity);
        newCube.transform.localScale = Vector3.one * 0.6f;
        Destroy(_currentMainCube.gameObject);
        Destroy(cube.gameObject);

        _currentMainCube = null;
        _rope.gameObject.SetActive(false);
        _rope.collisions.enabled = true;
    }
}
