using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRopeGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _originalPrefab;
    [SerializeField] private Transform _head;
    [SerializeField] private float _step;
    [SerializeField] private float _count;

    private List<Rigidbody> _currentLinks = new List<Rigidbody>();

    [Button]
    private void Generate()
    {
       if(_currentLinks.Count==0) _currentLinks.Add(_head.GetComponent<Rigidbody>());
        for(int i = 1; i <= _count; i++)
        {
            var newPart = Instantiate(_originalPrefab,_head.position + Vector3.forward*_step*i,Quaternion.identity,null);
            newPart.GetComponent<HingeJoint>().connectedBody = _currentLinks[i - 1];
            _currentLinks.Add(newPart.GetComponent<Rigidbody>());
        }
    }
    [Button]
    private void Clear()
    {
        foreach(var link in _currentLinks)
        {
            if(link.gameObject!=_head.gameObject) DestroyImmediate(link.gameObject);
        }
        _currentLinks = new List<Rigidbody>();
        _currentLinks.Add(_head.GetComponent<Rigidbody>());
    }
}
