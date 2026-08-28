using UnityEngine;

[DisallowMultipleComponent]
public class BAHeroCameraController : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _minimumX = -6f;
    [SerializeField] private float _maximumX = 6f;
    [SerializeField, Min(0f)] private float _smoothTime;
    [SerializeField] private float _targetOffsetX;

    private float _initialY;
    private float _initialZ;
    private float _xVelocity;

    private void Awake()
    {
        Vector3 initialPosition = transform.position;
        _initialY = initialPosition.y;
        _initialZ = initialPosition.z;
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            return;
        }

        float minimumX = Mathf.Min(_minimumX, _maximumX);
        float maximumX = Mathf.Max(_minimumX, _maximumX);
        float desiredCameraX = _target.position.x + _targetOffsetX;
        float targetX = Mathf.Clamp(desiredCameraX, minimumX, maximumX);
        float cameraX;

        if (_smoothTime > 0f)
        {
            cameraX = Mathf.SmoothDamp(
                transform.position.x,
                targetX,
                ref _xVelocity,
                _smoothTime,
                Mathf.Infinity,
                Time.deltaTime);
        }
        else
        {
            cameraX = targetX;
            _xVelocity = 0f;
        }

        transform.position = new Vector3(cameraX, _initialY, _initialZ);
    }
}
