using UnityEngine;

public static class CameraExtensions
{
    public static Bounds OrthographicBounds(this Camera camera)
    {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = camera.orthographicSize * 2.5f;
        Vector3 noZTransform = camera.transform.position;
        noZTransform.z = 0.0f;
        Bounds bounds = new Bounds(
            noZTransform,
            new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
        return bounds;
    }

    public static bool ActorOnScreen(this Camera camera,Transform transform)
    {
        var bounds = OrthographicBounds(camera);
        return transform.position.x > bounds.min.x ||
               transform.position.y > bounds.min.y ||
               transform.position.x < bounds.max.x ||
               transform.position.y < bounds.max.y;
    }
}