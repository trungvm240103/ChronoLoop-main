using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject player;   // Tham chiếu đến đối tượng player
    private Vector3 offset;     // Khoảng cách giữa camera và player

    public float smoothSpeed = 0.125f; // Tốc độ di chuyển mượt mà
    public float minX = -10f, maxX = 10f; // Giới hạn theo trục X
    public float minY = -5f, maxY = 5f;  // Giới hạn theo trục Y

    void Start()
    {
        // Tính toán khoảng cách ban đầu giữa camera và player
        if (player != null)
        {
            offset = transform.position - player.transform.position;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Tính toán vị trí mong muốn của camera
        Vector3 desiredPosition = player.transform.position + offset;

        // Giới hạn vị trí camera trong phạm vi cho phép
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);

        // Di chuyển camera một cách mượt mà
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Cập nhật vị trí camera
        transform.position = smoothedPosition;
    }
}
