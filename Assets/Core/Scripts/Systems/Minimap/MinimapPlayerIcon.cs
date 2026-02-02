using UnityEngine;

public class MinimapPlayerIcon : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        transform.localEulerAngles = new Vector3(0, 0, -player.eulerAngles.z);
    }
}
