using UnityEngine;

public class MouseInput : IInputHandler
{
    public float Horizontal => Input.GetAxis("Mouse X");
    public float Vertical => Input.GetAxis("Mouse Y");
    public bool Attack => Input.GetMouseButtonDown(0);
}
