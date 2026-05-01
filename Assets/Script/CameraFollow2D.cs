using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow")]
    public float smoothTime = 0.2f;
    public Vector3 offset;

    [Header("Zoom")]
    public Camera cam;
    public float normalZoom = 5f;
    public float zoomedInSize = 3f;
    public float zoomSmooth = 5f;

    float currentZoom;
    Vector3 velocity = Vector3.zero;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        currentZoom = normalZoom;
        cam.orthographicSize = normalZoom;
    }

    void LateUpdate()
    {
        if (target == null) return;

        FollowPlayer();
        HandleZoom();
    }

    void FollowPlayer()
    {
        Vector3 targetPos = target.position + offset;

        Vector3 smoothPos = Vector3.SmoothDamp(
            transform.position,
            new Vector3(targetPos.x, targetPos.y, transform.position.z),
            ref velocity,
            smoothTime
        );

        transform.position = smoothPos;
    }

    void HandleZoom()
    {
        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            currentZoom,
            Time.deltaTime * zoomSmooth
        );
    }

   
    public void ZoomIn()
    {
        currentZoom = zoomedInSize;
    }

   
    public void ZoomOut()
    {
        currentZoom = normalZoom;
    }

    public void SetZoom(float size)
    {
        currentZoom = size;
    }
}
