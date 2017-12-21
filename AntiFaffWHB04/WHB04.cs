using AntiFaffHID;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AntiFaffWHB04
{
    public delegate void EventHandler();

    public class Device
    {
        HidDevice inputDevice = new HidDevice(6);
        HidDevice outputDevice = new HidDevice(8);
        public event EventHandler<HB04InputEventArgs> Input;
        private Task receiveTask;
        private CancellationTokenSource cancelTokenSource;

        public bool Open()
        {
            //WHB04:
            //VID: 0x10CE
            //PID: 0xEB70

            Close();

            var devices = HidBrowse.Browse();
            if (devices.Count(d => d.Vid == 4302 && d.Pid == -5264) == 2)
            {
                var hb04Devices = devices.Where(d => d.Vid == 4302 && d.Pid == -5264);
                if (!inputDevice.Open(hb04Devices.First(d => d.Capabilities.InputReportByteLength == 6)))
                    throw new Exception("Could not open device");
                if (!outputDevice.Open(hb04Devices.First(d => d.Capabilities.FeatureReportByteLength == 8)))
                    throw new Exception("Could not open device");
            }
            else
                return false;

            cancelTokenSource = new CancellationTokenSource();
            receiveTask = new Task(() => ReceiveTask(cancelTokenSource.Token), TaskCreationOptions.LongRunning);
            receiveTask.Start();

            return true;
        }

        public void Close()
        {
            cancelTokenSource?.Cancel();
            if (inputDevice.State == HidDeviceState.Open)
            {
                inputDevice.Close();
            }

            if (outputDevice.State == HidDeviceState.Open)
            {
                outputDevice.Close();
            }
        }

        public void Write(OutputReport output)
        {
            Validate(output);
            var workXBytes = GetBytes(output.WorkX);
            var workYBytes = GetBytes(output.WorkY);
            var workZBytes = GetBytes(output.WorkZ);
            var machineXBytes = GetBytes(output.MachineX);
            var machineYBytes = GetBytes(output.MachineY);
            var machineZBytes = GetBytes(output.MachineZ);
            var feedrateOverrideBytes = GetBytes(output.FeedrateOverride);
            var spindleSpeedOverrideBytes = GetBytes(output.SpindleSpeedOverride);
            var feedrateBytes = GetBytes(output.Feedrate);
            var spindleSpeedBytes = GetBytes(output.SpindleSpeed);
            outputDevice.WriteFeature(new byte[] { 0x06, 0xFE, 0xFD, 0xFF, workXBytes[1], workXBytes[0], workXBytes[3], workXBytes[2] });
            outputDevice.WriteFeature(new byte[] { 0x06, workYBytes[1], workYBytes[0], workYBytes[3], workYBytes[2], workZBytes[1], workZBytes[0], workZBytes[3] });
            outputDevice.WriteFeature(new byte[] { 0x06, workZBytes[2], machineXBytes[1], machineXBytes[0], machineXBytes[3], machineXBytes[2], machineYBytes[1], machineYBytes[0] });
            outputDevice.WriteFeature(new byte[] { 0x06, machineYBytes[3], machineYBytes[2], machineZBytes[1], machineZBytes[0], machineZBytes[3], machineZBytes[2], feedrateOverrideBytes[1] });
            outputDevice.WriteFeature(new byte[] { 0x06, feedrateOverrideBytes[0], spindleSpeedOverrideBytes[1], spindleSpeedOverrideBytes[0], feedrateBytes[1], feedrateBytes[0], spindleSpeedBytes[1], spindleSpeedBytes[0] });
            outputDevice.WriteFeature(new byte[] { 0x06, (byte)((byte)output.StepMultiplier | (byte)output.ScreenGlyph), (byte)output.Misc, 0x00, 0x00, 0x00, 0x00, 0x00 });
        }

        protected virtual void OnInput(HB04InputEventArgs e)
        {
            Input?.Invoke(this, e);
        }

        private void Validate(OutputReport output)
        {
            if (output.WorkX > 9999.999m || output.WorkX < -9999.999m)
                throw new Exception("X value out of range");
            if (output.WorkY > 9999.999m || output.WorkY < -9999.999m)
                throw new Exception("Y value out of range");
            if (output.WorkZ > 9999.999m || output.WorkZ < -9999.999m)
                throw new Exception("Z value out of range");
        }

        private byte[] GetBytes(decimal value)
        {
            byte[] output = new byte[4];
            UInt16 x = (UInt16)Math.Truncate(Math.Abs(value));
            var xIntBytes = GetBytes(x);
            UInt16 xFractional = (UInt16)((Math.Abs(value) - Math.Truncate(Math.Abs(value))) * 10000);
            if (value < 0)
                xFractional = (UInt16)(xFractional | 0x8000);
            var xFractionalBytes = GetBytes(xFractional);

            output[0] = xIntBytes[0];
            output[1] = xIntBytes[1];
            output[2] = xFractionalBytes[0];
            output[3] = xFractionalBytes[1];
            return output;
        }

        private byte[] GetBytes(UInt16 value)
        {
            byte[] intBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            return intBytes;
        }

        private void ReceiveTask(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                if (inputDevice.Read(500, out byte[] data))
                    OnInput(new HB04InputEventArgs(new InputReport(data)));
            }
        }
    }

    public class HB04InputEventArgs : EventArgs
    {
        public InputReport InputReport { get; }

        public HB04InputEventArgs(InputReport report)
        {
            InputReport = report;
        }
    }
}
