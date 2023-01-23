using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform _ropeHeadTransform;

    [SerializeField] private InteractableCube _currentMainCube;
    [SerializeField] private bool _isHolding;

    private void Update()
    {
        if (Input.GetMouseButton(0) && _isHolding)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, LayerMask.GetMask("Player")) && hit.collider.CompareTag("Plane"))
            {
                var pos = hit.point;
                _ropeHeadTransform.position = pos;
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
                    _isHolding = true;
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (_currentMainCube != null)
            {
                _isHolding = false;
                _currentMainCube = null;
            }
        }
    }
}
