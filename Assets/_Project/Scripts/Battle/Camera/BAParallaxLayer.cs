using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(100)]
public class BAParallaxLayer : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform;
    [SerializeField, Range(0f, 1f)] private float _horizontalFollowFactor;

    private float _initialCameraX;
    private float _initialLayerX;
    private float _initialLayerY;
    private float _initialLayerZ;

    private void Awake()
    {
        Vector3 initialLayerPosition = transform.position;
        _initialLayerX = initialLayerPosition.x;
        _initialLayerY = initialLayerPosition.y;
        _initialLayerZ = initialLayerPosition.z;

        if (_cameraTransform == null)
        {
            return;
        }

        _initialCameraX = _cameraTransform.position.x;
    }

    private void LateUpdate()
    {
        if (_cameraTransform == null)
        {
            return;
        }

        float cameraDeltaX = _cameraTransform.position.x - _initialCameraX;
        float layerX =
            _initialLayerX +
            cameraDeltaX * Mathf.Clamp01(_horizontalFollowFactor);

        transform.position = new Vector3(
            layerX,
            _initialLayerY,
            _initialLayerZ);
    }
}
