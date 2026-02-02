using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        Vector3 pos = player.position;
        pos.z = transform.position.z; // keep camera height
        transform.position = pos;
    }
}
