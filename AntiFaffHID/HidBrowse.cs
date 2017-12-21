using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace AntiFaffHID
{
    public class HidBrowse
    {
        public static List<HidInfo> Browse()
        {
            Guid gHid;
            List<HidInfo> info = new List<HidInfo>();

            // Obtain hid guid
            Native.HidD_GetHidGuid(out gHid);
            // Get list of present hid devices
            var hInfoSet = Native.SetupDiGetClassDevs(ref gHid, null, IntPtr.Zero, Native.DIGCF_DEVICEINTERFACE | Native.DIGCF_PRESENT);

            // Allocate mem for interface descriptor
            var iface = new Native.DeviceInterfaceData();
            // Set size field
            iface.Size = Marshal.SizeOf(iface);
            // Interface index
            uint index = 0;

            // iterate through all interfaces
            while (Native.SetupDiEnumDeviceInterfaces(hInfoSet, 0, ref gHid, index, ref iface))
            {
                short vid, pid;

                var path = GetPath(hInfoSet, ref iface);

                var handle = Open(path);
                if (handle != Native.INVALID_HANDLE_VALUE)
                {
                    var man = GetManufacturer(handle);
                    var prod = GetProduct(handle);
                    var serial = GetSerialNumber(handle);
                    GetVidPid(handle, out vid, out pid);
                    var cap = GetDeviceCapabilities(handle);

                    HidInfo i = new HidInfo(prod, serial, man, path, vid, pid, cap);
                    info.Add(i);

                    Close(handle);
                }

                index++;
            }

            // Clean up
            if (Native.SetupDiDestroyDeviceInfoList(hInfoSet) == false)
            {
                throw new Win32Exception();
            }

            return info;
        }

        private static IntPtr Open(string path)
        {
            return Native.CreateFile(path,
                Native.GENERIC_READ | Native.GENERIC_WRITE,
                Native.FILE_SHARE_READ | Native.FILE_SHARE_WRITE,
                IntPtr.Zero, Native.OPEN_EXISTING, Native.FILE_FLAG_OVERLAPPED,
                IntPtr.Zero);
            // If for some reason not all devices are showing, try the following
            /* return Native.CreateFile(path,
                0,
                Native.FILE_SHARE_READ | Native.FILE_SHARE_WRITE,
                IntPtr.Zero,
                Native.OPEN_EXISTING,
                0,
                IntPtr.Zero); */
        }

        private static void Close(IntPtr handle)
        {
            if (Native.CloseHandle(handle) == false)
            {
                throw new Win32Exception();
            }
        }

        private static string GetPath(IntPtr hInfoSet, ref Native.DeviceInterfaceData iface)
        {
            var detIface = new Native.DeviceInterfaceDetailData();
            uint reqSize = (uint)Marshal.SizeOf(detIface);

            /* Set size. The cbSize member always contains the size of the 
             * fixed part of the data structure, not a size reflecting the 
             * variable-length string at the end. */
            detIface.Size = Marshal.SizeOf(typeof(IntPtr)) == 8 ? 8 : 5;

            bool status = Native.SetupDiGetDeviceInterfaceDetail(hInfoSet, ref iface, ref detIface, reqSize, ref reqSize, IntPtr.Zero);

            if (!status)
            {
                throw new Win32Exception();
            }

            return detIface.DevicePath;
        }

        private static string GetManufacturer(IntPtr handle)
        {
            var s = new StringBuilder(256);
            string rc = String.Empty;

            if (Native.HidD_GetManufacturerString(handle, s, s.Capacity))
            {
                rc = s.ToString();
            }

            return rc;
        }

        private static string GetProduct(IntPtr handle)
        {
            var s = new StringBuilder(256);
            string rc = String.Empty;

            if (Native.HidD_GetProductString(handle, s, s.Capacity))
            {
                rc = s.ToString();
            }

            return rc;
        }

        private static string GetSerialNumber(IntPtr handle)
        {
            var s = new StringBuilder(256);
            string rc = String.Empty;

            if (Native.HidD_GetSerialNumberString(handle, s, s.Capacity))
            {
                rc = s.ToString();
            }

            return rc;
        }

        private static void GetVidPid(IntPtr handle, out short Vid, out short Pid)
        {
            var attr = new Native.HiddAttributtes();
            attr.Size = Marshal.SizeOf(attr);

            if (Native.HidD_GetAttributes(handle, ref attr) == false)
            {
                throw new Win32Exception();
            }

            Vid = attr.VendorID; Pid = attr.ProductID;
        }

        private static HidDeviceCapabilities GetDeviceCapabilities(IntPtr handle)
        {
            var capabilities = default(Native.HIDP_CAPS);
            var preparsedDataPointer = default(IntPtr);

            if (Native.HidD_GetPreparsedData(handle, ref preparsedDataPointer))
            {
                Native.HidP_GetCaps(preparsedDataPointer, ref capabilities);
                Native.HidD_FreePreparsedData(preparsedDataPointer);
            }
            return new HidDeviceCapabilities(capabilities);
        }
    }
}
