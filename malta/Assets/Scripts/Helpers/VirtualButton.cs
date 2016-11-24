using UnityEngine;
using InControl;

public class VirtualButton
{
    private bool _isKeyDown;
    private bool _isKeyUp;
    private bool _isPressed;
    private float min;
    private float max;
    public bool BtnDown
    {
        get
        {
            return _isKeyDown;
        }
    }
    public bool BtnUp
    {
        get
        {
            return _isKeyUp;
        }
    }
    public bool Pressed
    {
        get
        {
            return (_isPressed);
        }
    }
    private bool isInControl;
    private bool isNegative;
    private KeyCode _key;
    private InputDevice device;
    private InputControl _control;

    public VirtualButton(KeyCode key)
    {
        isInControl = false;
        _key = key;
    }

    public VirtualButton(InputControlType control)
    {
        isInControl = true;
        device = InputManager.ActiveDevice;
        _control = device.GetControl(control);
    }

    public void Update()
    {
        if (isInControl == true)
        {
            if (InputManager.ActiveDevice != device)
            {
                device = InputManager.ActiveDevice;
            }
            if (_control.IsButton == false)
            {
                if ((isNegative == true && _control.Value < min) || (isNegative == false && _control.Value > min))
                {
                    if (_isPressed == false)
                    {
                        _isKeyDown = true;
                        _isKeyUp = false;
                    }
                    else
                    {
                        _isKeyDown = false;
                    }
                    _isPressed = true;
                }
                else
                {
                    if (_isPressed == true)
                    {
                        _isKeyUp = true;
                        _isKeyDown = false;
                    }
                    else
                    {
                        _isKeyUp = false;
                    }
                    _isPressed = false;
                }
            }
            else
            {
                if (_isKeyDown == true)
                {
                    _isKeyDown = false;
                }
                else if (_isPressed == false && _control.IsPressed == true)
                {
                    _isKeyDown = true;
                }
                if (_isKeyUp == true)
                {
                    _isKeyUp = false;
                }
                else if (_isPressed == true && _control.IsPressed == false)
                {
                    _isKeyUp = true;
                }
                _isPressed = _control.IsPressed;
            }
        }
        else
        {
            _isKeyDown = Input.GetKeyDown(_key);
            _isKeyUp = Input.GetKeyUp(_key);
            _isPressed = Input.GetKey(_key);
        }

    }
}