using UnityEngine;
using System.Collections;
using InControl;

public class HardwareInterfaceManager : Manager <HardwareInterfaceManager>
{
    private VirtualButton _left;
    private VirtualButton _right;
    private VirtualButton _up;
    private VirtualButton _down;
    public VirtualButton Confirm;
    public VirtualButton Cancel;
    public VirtualStick LeftStick;
    public VirtualStick RightStick;
    public VirtualButtonMultiplexer Left;
    public VirtualButtonMultiplexer Right;
    public VirtualButtonMultiplexer Up;
    public VirtualButtonMultiplexer Down;

    void Awake ()
    {
        RefreshVirtualButtons();
    }

    void Update ()
    {
        Left.Update();
        Right.Update();
        Up.Update();
        Down.Update();
        Confirm.Update();
        Cancel.Update();
    }

    void RefreshVirtualButtons ()
    {
        _left = new VirtualButton(KeyCode.LeftArrow);
        _right = new VirtualButton(KeyCode.RightArrow);
        _up = new VirtualButton(KeyCode.UpArrow);
        _down = new VirtualButton(KeyCode.DownArrow);
        Confirm = new VirtualButton(KeyCode.Return);
        Cancel = new VirtualButton(KeyCode.Backspace);
        Left = new VirtualButtonMultiplexer(_left);
        Right = new VirtualButtonMultiplexer(_right);
        Up = new VirtualButtonMultiplexer(_up);
        Down = new VirtualButtonMultiplexer(_down);
    }

}
