using System;

namespace AntiFaffHID
{
    public class HidInfo
    {
        public string Path { get; private set; }            // Device path
        public short Vid { get; private set; }              // Vendor ID
        public short Pid { get; private set; }              // Product ID
        public string Product { get; private set; }         // USB product string
        public string Manufacturer { get; private set; }    // USB manufacturer string
        public string SerialNumber { get; private set; }    // USB serial number string
        public HidDeviceCapabilities Capabilities { get; private set; }     //Capabilities.Usage is often useful for picking the exact HID interface required

        public HidInfo(string product, string serialNumber, string manufacturer, string path, short vid, short pid, HidDeviceCapabilities capabilities)
        {
            Product = product;
            SerialNumber = serialNumber;
            Manufacturer = manufacturer;
            Path = path;
            Vid = vid;
            Pid = pid;
            Capabilities = capabilities;
        }
    }
}
