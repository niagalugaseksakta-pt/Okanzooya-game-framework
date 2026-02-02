public class JoystickInput : IInputHandler
{
    private JoystickController joystick;
    private bool attackButtonPressed;

    public JoystickInput(JoystickController joystick)
    {
        this.joystick = joystick;
    }

    public float Horizontal => joystick.Horizontal;
    public float Vertical => joystick.Vertical;

    public bool Attack
    {
        get
        {
            bool temp = attackButtonPressed;
            attackButtonPressed = false;
            return temp;
        }
    }

    public void OnAttackButtonPressed()
    {
        attackButtonPressed = true;
    }
}
