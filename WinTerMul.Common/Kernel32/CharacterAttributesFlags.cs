using System;

namespace WinTerMul.Common.Kernel32
{
    [Flags]
    public enum CharacterAttributesFlags : ushort
    {
        None = 0,
        ForegroundBlue = 1,
        ForegroundGreen = 2,
        ForegroundRed = 4,
        ForegroundIntensity = 8,
        BackgroundBlue = 16,
        BackgroundGreen = 32,
        BackgroundRed = 64,
        BackgroundIntensity = 128,
        CommonLvbLeadingByte = 256,
        CommonLvbTrailingByte = 512,
        CommonLvbGridHorizontal = 1024,
        CommonLvbGridLvertical = 2048,
        CommonLvbGridRvertical = 4096,
        CommonLvbReverseVideo = 16384,
        CommonLvbUnderscore = 32768
    }
}
