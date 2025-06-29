using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float distance = 4f; // Camera distance from the behind the target (x-axis)
    public float height = 2f; // Camera height above the target (y-axis)
    public float rotationDamping = 3f; // Speed of camera rotation
    public float angle = 1.5f; // Angle of the camera rotation around the target
    private Transform target; // The target to follow
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = transform.parent;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 targetPosition = target.position + new Vector3(0, height, -distance);

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * rotationDamping);
        transform.LookAt(target.position + Vector3.up * angle);
    }
}
