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

    [SerializeField] private InteractableCube _currentMainCube;
    [SerializeField] private bool _isHolding;

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
                    _rope.ResetToSpawnCurve();
                    _currentMainCube = cube;
                    _ropeTailConnection.transformSettings.transform = cube.RopeAttachmentPoint;
                    _isHolding = true;
                    _currentMainCube.Collider.enabled = false;
                    _rope.gameObject.SetActive(true);
                    //_ropeHeadConnection.rigidbodySettings.body = cube.Rigidbody;
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
                    if (hit.collider.TryGetComponent<InteractableCube>(out var cube) && cube.gameObject!=_currentMainCube.gameObject)
                    {
                        //_ropeHeadConnection.type = RopeConnectionType.PinTransformToRope;
                        _ropeHeadConnection.transformSettings.transform = cube.transform;
                        StartCoroutine(MoveCubeAlongTheRope(cube.transform));
                        //_ropeHeadConnection.rigidbodySettings.body= cube.Rigidbody;
                    }
                    else _rope.gameObject.SetActive(false);
                }
                else _rope.gameObject.SetActive(false);


                _isHolding = false;
                _currentMainCube = null;
            }
        }

    }
    private IEnumerator MoveCubeAlongTheRope(Transform cube)
    {
        List<Vector3> positions = new List<Vector3>();
        for (int i = _rope.measurements.particleCount - 1; i >= 0; i--)
        {
            var pos = new Vector3(_rope.GetPositionAt(i).x,0.5f, _rope.GetPositionAt(i).z);
            positions.Add(pos);
        }

        float delay = 0.01f;
        var mainAnchor = _currentMainCube.RopeAttachmentPoint;
        _rope.collisions.enabled = false;
        DOVirtual.DelayedCall(0.2f, () =>
        {
            mainAnchor.DOMoveY(-2, 0.6f);
        });
        for (int i =0; i <= _rope.measurements.particleCount - 1; i++)
        {
            cube.DOMove(positions[i], delay);
            yield return new WaitForSeconds(delay);
        }
    }
}
