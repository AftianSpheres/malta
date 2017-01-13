using System;

public enum ProgressionFlags : int // signed int for bitflags is eww but Unity throws tantrums when asked to serialize anything else for some reason.
{
    None = 0,
    Tutorial0_IntroCutscenePlayed = 1 << 0,
    Tutorial0_Complete = 1 << 1,
    Tutorial1_IntroCutscenePlayed = 1 << 2,
    Tutorial1_Complete = 1 << 3,
    TaskmasterUnlock = 1 << 4,
    TowerUnlock = 1 << 5,
    FaeUnlock = 1 << 6,
    OrcUnlock = 1 << 7,

}
