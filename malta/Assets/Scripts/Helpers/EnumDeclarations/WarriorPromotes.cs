using System;

[Flags]
public enum WarriorPromotes
{
    None = 0,
    Bowman = 1 << 0,
    Footman = 1 << 1
}

public static class WarriorPromotes_Metadata
{
    public const int LastVal = (int) WarriorPromotes.Footman;
}