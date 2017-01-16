using System;

[Flags]
public enum MysticPromotes
{
    None = 0,
    Sage = 1 << 0,
    Wizard = 1 << 1,
}
