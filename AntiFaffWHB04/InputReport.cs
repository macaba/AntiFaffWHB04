using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiFaffWHB04
{
    public class InputReport
    {
        private uint ReportID { get; }
        public Button Button1 { get; }
        public Button Button2 { get; }
        public WheelMode WheelMode { get; }
        public int Wheel { get; }
        private bool On { get; }

        public InputReport(byte[] data)
        {
            if (data.Count() != 6)
                throw new Exception("Invalid data");

            ReportID = data[0];
            Button1 = (Button)data[1];
            Button2 = (Button)data[2];
            WheelMode = (WheelMode)data[3];
            Wheel = (sbyte)data[4];
            On = data[5] > 0;
        }

        public override string ToString()
        {
            return String.Format("Button 1: {0,16}, Button 2: {1,16}, Wheel Mode: {2,7}, Wheel: {3,4}, On: {4,5}",
                                      Button1.ToString(),
                                      Button2.ToString(),
                                      WheelMode.ToString(),
                                      Wheel.ToString(),
                                      On.ToString());
        }
    }

    public enum Button
    {
        None = 0,
        Reset = 23,
        Stop = 22,
        Home = 1,
        StartPause = 2,
        Rewind = 3,
        ProbeZ = 4,
        Spindle = 12,
        FeedOverrideHalf = 6,
        FeedOverrideZero = 7,
        SafeZ = 8,
        WorkZero = 9,
        Macro1 = 10,
        Macro2 = 11,
        Macro3 = 5,
        Step = 13,
        MPG = 14,
        Macro6 = 15,
        Macro7 = 16
    }

    public enum WheelMode
    {
        Off = 0,
        X = 17,
        Y = 18,
        Z = 19,
        A = 24,
        Spindle = 20,
        Feed = 21
    }
}
