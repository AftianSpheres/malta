using UnityEngine;
using InControl;

/// <summary>
/// Maps stick inputs and buttons to the same interface.
/// Used for directional vbuttons, mostly.
/// </summary>
public class VirtualButtonMultiplexer
{

    private bool _isKeyDown;
    private bool _isKeyUp;
    private bool _isPressed;
    private VirtualButton _btn;
    private VirtualStick _stick;
    private bool _isYAxis;
    private bool _isNegative;

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

    public VirtualButtonMultiplexer(VirtualButton btn, VirtualStick stick = default(VirtualStick), bool isYAxis = false, bool isNegative = false)
    {
        _btn = btn;
        _stick = stick;
        _isYAxis = isYAxis;
        _isNegative = isNegative;
    }

    public void Update()
    {
        bool stickInUse = _stick != null && (Mathf.Abs(_stick.x) > 0.2f || Mathf.Abs(_stick.y) > 0.2f);
        bool _rawIsPressed;
        if (_isYAxis == true && stickInUse)
        {
            if (_isNegative == true)
            {
                _rawIsPressed = (_stick.y < -0.2f);
            }
            else
            {
                _rawIsPressed = (_stick.y > 0.2f);
            }
        }
        else if (_isYAxis == false && stickInUse)
        {
            if (_isNegative == true)
            {
                _rawIsPressed = (_stick.x < -0.2f);
            }
            else
            {
                _rawIsPressed = (_stick.x > 0.2f);
            }
        }
        else
        {
            _rawIsPressed = _btn.Pressed;
        }
        if (_isKeyDown == true)
        {
            _isKeyDown = false;
        }
        else if (_isPressed == false && _rawIsPressed == true)
        {
            _isKeyDown = true;
        }
        if (_isKeyUp == true)
        {
            _isKeyUp = false;
        }
        else if (_isPressed == true && _rawIsPressed == false)
        {
            _isKeyUp = true;
        }
        _isPressed = _rawIsPressed;
    }
}
