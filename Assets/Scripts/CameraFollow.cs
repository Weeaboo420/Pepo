using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform _target;
    private const float _speed = 7f;
    private Vector2 _minBounds, _maxBounds;

    private void Start()
    {
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        _minBounds = new Vector2(-1.6f, -11f);
        _maxBounds = new Vector2(7f, 0.5f);
    }

    private void Update()
    {
        //Confine the camera to a rectangular area defined by _minBounds and _maxBounds
        Vector2 newPos = _target.transform.position;
        newPos.x = Mathf.Clamp(newPos.x, _minBounds.x, _maxBounds.x);
        newPos.y = Mathf.Clamp(newPos.y, _minBounds.y, _maxBounds.y);

        //Smoothly move towards "newPos"
        transform.position = Vector3.Lerp(transform.position, new Vector3(newPos.x, newPos.y, transform.position.z), Time.deltaTime * _speed);    
    }
}
