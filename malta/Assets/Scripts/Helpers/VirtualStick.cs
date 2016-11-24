using UnityEngine;
using InControl;

public class VirtualStick
{
    private InputControlType xcontrolType;
    private InputControlType ycontrolType;
    private InputControl xcontrol;
    private InputControl ycontrol;
    private InputDevice device;
    private float _x;
    private float _y;
    private float deadZone;
    private float sensitivity;
    private bool invertX;
    private bool invertY;

    public float x
    {
        get
        {
            return _x;
        }
    }

    public float y
    {
        get
        {
            return _y;
        }
    }

    public void Update()
    {
        if (InputManager.ActiveDevice != device)
        {
            device = InputManager.ActiveDevice;
        }
        _x = device.GetControl(xcontrolType).Value;
        _y = device.GetControl(ycontrolType).Value;
        if (Mathf.Abs(_x) + Mathf.Abs(_y) < deadZone)
        {
            _x = 0;
            _y = 0;
        }
        _x *= sensitivity * sensitivity * Mathf.Abs(_x / sensitivity);
        _y *= sensitivity * sensitivity * Mathf.Abs(_y / sensitivity);
        if (invertX == true)
        {
            _x *= -1;
        }
        if (invertY == true)
        {
            _y *= -1;
        }
        _x /= (1f - deadZone);
    }

    public VirtualStick(InputControlType _xcontrol, InputControlType _ycontrol, float _deadZone, float _sensitivity, bool _invertX, bool _invertY)
    {
        xcontrolType = _xcontrol;
        ycontrolType = _ycontrol;
        deadZone = _deadZone;
        sensitivity = _sensitivity;
        invertX = _invertX;
        invertY = _invertY;
        device = InputManager.ActiveDevice;
    }

}