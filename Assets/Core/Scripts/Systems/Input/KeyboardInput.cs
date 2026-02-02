using UnityEngine;

public class KeyboardInput : IInputHandler
{
    public float Horizontal => Input.GetAxis("Horizontal");
    public float Vertical => Input.GetAxis("Vertical");
    public bool Attack => Input.GetKeyDown(KeyCode.Space);
}
