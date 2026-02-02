using UnityEngine;

public class TouchInput : IInputHandler
{
    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }
    public bool Attack { get; private set; }

    public void Update()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            Horizontal = touch.deltaPosition.x / Screen.width;
            Vertical = touch.deltaPosition.y / Screen.height;
            Attack = touch.tapCount > 1;
        }
        else
        {
            Horizontal = Vertical = 0;
            Attack = false;
        }
    }
}
