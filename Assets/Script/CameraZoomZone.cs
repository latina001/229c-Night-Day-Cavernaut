using UnityEngine;

public class CameraZoomZone : MonoBehaviour
{
    public CameraFollow2D cam;

    [Header("Zoom Setting")]
    public float zoomSize = 3f;     // ¤èÒ zoom ¢Í§â«¹¹Õé
    public bool useCustomZoom = true;

    [Header("Exit Setting")]
    public bool resetOnExit = true;
    public float defaultZoom = 5f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && useCustomZoom)
        {
            cam.SetZoom(zoomSize);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && resetOnExit)
        {
            cam.SetZoom(defaultZoom);
        }
    }
}