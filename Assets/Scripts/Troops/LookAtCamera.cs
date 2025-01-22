using Core;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform _cam;
    
    private void Start()
    {
        _cam = GameManager.Camera.transform;
    }

   
    private void LateUpdate()
    {
        transform.LookAt(transform.position + _cam.forward);
    }
}
