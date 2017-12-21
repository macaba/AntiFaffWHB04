using System;

namespace AntiFaffWHB04
{
    public class OutputReport
    {
        public decimal WorkX;
        public decimal WorkY;
        public decimal WorkZ;

        public decimal MachineX;
        public decimal MachineY;
        public decimal MachineZ;

        public UInt16 FeedrateOverride;
        public UInt16 SpindleSpeedOverride;
        public UInt16 Feedrate;
        public UInt16 SpindleSpeed;

        public StepMultiplier StepMultiplier;
        public ScreenGlyph ScreenGlyph;
        public Misc Misc;
    }

    public enum StepMultiplier
    {
        Step0 = 0,
        Step1 = 1,
        Step5 = 2,
        Step10 = 3,
        Step20 = 4,
        Step30 = 5,
        Step40 = 6,
        Step50 = 7,
        Step100 = 8,
        Step500 = 9,
        Step1000 = 10
    }

    public enum ScreenGlyph
    {
        None = 0,
        Home = 80,
        WorkOrigin = 16,
        JogOn = 96,
        Reset = 32
    }

    public enum Misc
    {
        MM = 0x00,
        Inch = 0x80
    }
}
