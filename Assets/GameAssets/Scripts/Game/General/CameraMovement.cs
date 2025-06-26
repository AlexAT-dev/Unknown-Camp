using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public static CameraMovement Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [SerializeField] private Transform target;
    [SerializeField] private float damping;

    private Vector3 offset = new Vector3(0, 0, -10);
    private Vector3 velocity = Vector3.zero;
    
    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 movePosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, movePosition, ref velocity, damping);
    }

    public void SetTarget(Transform target, bool tpCamera)
    {
        if (tpCamera) transform.position = target.position;
        this.target = target;
    }
}
