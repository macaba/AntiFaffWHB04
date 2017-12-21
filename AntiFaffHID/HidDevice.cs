using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace AntiFaffHID
{
    public class HidDevice : IDisposable
    {
        private IntPtr handle;                              // Native HID handle
        private FileStream fileStream;                      // HID stream
        private Task receiveTask;                           // HID read task
        private BlockingCollection<byte[]> receiveQueue;    // HID read queue
        private HidDeviceState hidDeviceState = HidDeviceState.Closed;
        private UInt16 readReportLength;

        public HidDeviceState State
        {
            get { return hidDeviceState; }
        }

        //public FileStream FileStream
        //{
        //    get { return fileStream; }
        //    // do not expose this setter
        //    internal set { fileStream = value; }
        //}

        public HidDevice(UInt16 readReportLength)
        {
            this.readReportLength = readReportLength;
        }

        public bool Open(HidInfo hidInfo)
        {
            Close();

            SafeFileHandle shandle;

            // Open HID device file
            handle = Native.CreateFile(hidInfo.Path,
                Native.GENERIC_READ | Native.GENERIC_WRITE,
                Native.FILE_SHARE_READ | Native.FILE_SHARE_WRITE,
                IntPtr.Zero, Native.OPEN_EXISTING, Native.FILE_FLAG_OVERLAPPED,
                IntPtr.Zero);

            // Return failure if handle is invalid
            if (handle == Native.INVALID_HANDLE_VALUE)
            {
                return false;
            }

            // Open safe file handle
            shandle = new SafeFileHandle(handle, false);

            // Prepare stream - async
            fileStream = new FileStream(shandle, FileAccess.ReadWrite, 32, true);

            // Start asyncronous receive task
            receiveQueue = new BlockingCollection<byte[]>(1000);
            if (receiveTask != null)
                if (!receiveTask.IsCompleted)
                    throw new Exception("Task already running");
            receiveTask = new Task(() => ReceiveTask(receiveQueue), TaskCreationOptions.LongRunning);
            receiveTask.Start();

            hidDeviceState = HidDeviceState.Open;

            // Return success
            return true;
        }

        public void Close()
        {
            if (hidDeviceState == HidDeviceState.Open)
            {
                // Stop Receive Task
                if (!receiveQueue.IsCompleted)
                    receiveQueue.CompleteAdding();

                // Handle filestream
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream = null;
                }

                // Close native HID handle
                Native.CloseHandle(handle);

                hidDeviceState = HidDeviceState.Closed;
            }
        }

        public void Write(byte[] data)
        {
            fileStream.Write(data, 0, data.Length);
            fileStream.Flush();
        }

        public void WriteFeature(byte[] data)
        {
            if (!Native.HidD_SetFeature(handle, data, data.Length))
                throw new Exception("Could not set HID feature");
        }

        public bool Read(int timeout, out byte[] data)
        {
            if (receiveTask.Status == TaskStatus.Running)
                return receiveQueue.TryTake(out data, timeout);
            else
                throw new HidDeviceUnavailableException();
        }

        private void ReceiveTask(BlockingCollection<byte[]> receiveQueue)
        {
            while (!receiveQueue.IsCompleted)
            {
                try
                {
                    byte[] data = new byte[readReportLength];
                    if (ReadPrivate(data))
                        receiveQueue.Add(data);
                }
                catch (OperationCanceledException ex)      //HIDDev is closing
                {
                    Close();
                }
                catch (IOException ex)                     //Device was disconnected
                {
                    Close();
                }
            }
        }

        private bool ReadPrivate(byte[] data)
        {
            if (fileStream != null)
            {
                int n = 0, bytes = data.Length;

                while (n != bytes)
                {
                    int rc = fileStream.Read(data, n, bytes - n);
                    n += rc;
                }
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            Close();
        }
    }

    public enum HidDeviceState
    {
        Open,
        Closed
    }
}
