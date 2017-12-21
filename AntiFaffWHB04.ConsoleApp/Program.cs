using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AntiFaffWHB04.ConsoleApp
{
    class Program
    {
        //This console app will send keyboard presses for use with Grbl Panel
        private static decimal x = 0;

        static void Main(string[] args)
        {
            Device whb04 = new Device();
            whb04.Input += Hb04_Input;
            if (whb04.Open())
            {
                Console.WriteLine("WHB04 found. Now listening for updates.");
                whb04.Write(new OutputReport()
                {
                    MachineX = 1,
                    MachineY = 2,
                    MachineZ = 3,
                    WorkX = 4,
                    WorkY = 5,
                    WorkZ = 6,
                    StepMultiplier = StepMultiplier.Step5,
                    ScreenGlyph = ScreenGlyph.JogOn,
                    SpindleSpeed = 7,
                    SpindleSpeedOverride = 8,
                    Feedrate = 9,
                    FeedrateOverride = 10
                });
                Console.ReadKey();
                whb04.Close();
            }
            else
            {
                Console.WriteLine("WHB04 not found. Press any key to close.");
                Console.ReadKey();
            }
        }

        private static void Hb04_Input(object sender, HB04InputEventArgs e)
        {
            var data = e.InputReport;
            x += (decimal)(data.Wheel * 10.0);
            Console.WriteLine(data.ToString());
            ((Device)sender).Write(new OutputReport()
            {
                MachineX = 12,
                MachineY = 34,
                MachineZ = 56,
                WorkX = x,
                WorkY = 90,
                WorkZ = 123,
                FeedrateOverride = 100,
                SpindleSpeedOverride = 100,
                Feedrate = UInt16.MaxValue
            });
            if (data.Wheel != 0)
                switch (data.WheelMode)
                {
                    case WheelMode.X:
                        for (int i = 0; i < Math.Abs(data.Wheel); i++)
                        {
                            if (data.Wheel > 0)
                                SendKeys.SendWait("{LEFT}");
                            else
                                SendKeys.SendWait("{RIGHT}");
                        }

                        break;
                    case WheelMode.Y:
                        for (int i = 0; i < Math.Abs(data.Wheel); i++)
                        {
                            if (data.Wheel > 0)
                                SendKeys.SendWait("{UP}");
                            else
                                SendKeys.SendWait("{DOWN}");
                        }
                        break;
                    case WheelMode.Z:
                        for (int i = 0; i < Math.Abs(data.Wheel); i++)
                        {
                            if (data.Wheel > 0)
                                SendKeys.SendWait("{PGUP}");
                            else
                                SendKeys.SendWait("{PGDN}");
                        }
                        break;
                }
            switch (data.Button1)
            {
                case Button.StartPause:
                    SendKeys.SendWait(" ");
                    break;
                case Button.Step:
                    SendKeys.SendWait("+");
                    break;
                case Button.MPG:
                    SendKeys.SendWait("-");
                    break;
            }
        }
    }
}
