

/* ***********************************************
 * File:    ArduinoUsbDevice.cs
 * Version: 20130120
 * Author:  tinozeegerman@gmail.com
 * License: CC-BY-SA (http://freedomdefined.org/Licenses/CC-BY-SA)
 * 
 * Description: This is pretty much a straight port of usbdevice.py from the DigiSpark Sample Code
 *              I've added notification for when the DigiSpark is connected and disconnected.
 *              You can use this code to communicate with your DigiSpark's DigiUSB interface
 *              To run successfully you need to install LibUsbDotNet: http://sourceforge.net/projects/libusbdotnet/
 *              Note that there seems to be a problem with LibDotNetUsb and Notifications on .Net 4.0,
 *              use 3.5 or below
 *              
 * ***********************************************/

using LibUsbDotNet;
using LibUsbDotNet.DeviceNotify;
using LibUsbDotNet.Main;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DigiSparkDotNet
{

    public class ArduinoUsbDevice
    {
        private UsbDeviceFinder MyUsbFinder;
        private UsbDevice usbDevice;
        private UsbEndpointReader usbReader;
        private UsbEndpointWriter usbWriter;


        private IDeviceNotifier UsbDeviceNotifier;
        private int VendorId;
        private int ProductId;

        public bool isAvailable;

        //This will be invoked every time a DigiSpark is connected or disconnected.
        public event EventHandler<EventArgs> ArduinoUsbDeviceChangeNotifier;

        //default values for the DigiSpark
        public ArduinoUsbDevice()
            : this(0x16c0, 0x05df)
        {
        }

        public ArduinoUsbDevice(int vendorId, int productId)
        {
            VendorId = vendorId;
            ProductId = productId;

            MyUsbFinder = new UsbDeviceFinder(VendorId, ProductId);
            UsbDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();

            UsbDeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;

            ConnectUsbDevice();
        }

        private void ConnectUsbDevice()
        {
            // Find and open the usb device.
            usbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);
            isAvailable = (usbDevice != null);

            //IUsbDevice wholeUsbDevice = usbDevice as IUsbDevice;
            //if (!ReferenceEquals(wholeUsbDevice, null))
            //{
            //    wholeUsbDevice.SetConfiguration(1);
            //    wholeUsbDevice.ClaimInterface(1);
            //}

            //usbReader = usbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
 
            //usbReader.DataReceived += usbReader_DataReceived;
            //usbReader.DataReceivedEnabled = true;

        }

        //void usbReader_DataReceived(object sender, EndpointDataEventArgs e)
        //{
        //    Debug.WriteLine(Encoding.Default.GetString(e.Buffer, 0, e.Count));
        //}

        private void OnDeviceNotifyEvent(object sender, DeviceNotifyEventArgs e)
        {
            if (e.Device.IdVendor == VendorId && e.Device.IdProduct == ProductId)
            {
                if (e.EventType == EventType.DeviceArrival)
                {
                    ConnectUsbDevice();
                    if (ArduinoUsbDeviceChangeNotifier != null)
                        ArduinoUsbDeviceChangeNotifier.Invoke(sender, e);
                }
                else if (e.EventType == EventType.DeviceRemoveComplete)
                {
                    usbDevice = null;
                    isAvailable = false;
                    if (ArduinoUsbDeviceChangeNotifier != null)
                        ArduinoUsbDeviceChangeNotifier.Invoke(sender, e);
                }
            }
        }


        public string GetStringDescriptor(byte index)
        {
            if (isAvailable == false)
                return null;

            UsbSetupPacket packet = new UsbSetupPacket((byte)UsbEndpointDirection.EndpointIn,
                                                        (byte)UsbStandardRequest.GetDescriptor,
                                                        (short)(0x0300 | index), // (usb.util.DESC_TYPE_STRING << 8) | index
                                                        0, //Language ID
                                                        255); //Length

            byte[] byteArray = new byte[256];
            int numBytesTransferred = 0;

            bool sendResult = usbDevice.ControlTransfer(ref packet, byteArray, byteArray.Length, out numBytesTransferred);
            string result = Encoding.Unicode.GetString(byteArray);

            return result;
        }

        public bool WriteByte(byte value)
        {
            if (isAvailable == false)
                return false;

            UsbSetupPacket packet = new UsbSetupPacket(
                (byte)(UsbCtrlFlags.RequestType_Class | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.Direction_Out),
                0x09, // USBRQ_HID_SET_REPORT 
                0x300, // (USB_HID_REPORT_TYPE_FEATURE << 8) | 0,
                value, // the byte to write
                0); // according to usbdevice.py this is ignored, so passing in 0

            byte[] writeBuffer = { 115 };

            int numBytesTransferred = 0;

            bool sendResult = usbDevice.ControlTransfer(ref packet, null, 0, out numBytesTransferred);
            if (!sendResult) Debug.WriteLine("Fail in control");
            return sendResult;

        }

        public bool WriteBytes(byte[] values)
        {
            if (isAvailable == false)
                return false;

            bool result = true;

            foreach (byte value in values)
            {
                result &= WriteByte(value);
            }

            return result;
        }

        public bool ReadByte(out byte[] value)
        {
            value = new byte[1];

            if (isAvailable == false)
                return false;

            UsbSetupPacket packet = new UsbSetupPacket(
                (byte)(UsbCtrlFlags.RequestType_Class | UsbCtrlFlags.Recipient_Device | UsbCtrlFlags.Direction_In),
                0x01, // USBRQ_HID_GET_REPORT 
                0x300, // (USB_HID_REPORT_TYPE_FEATURE << 8) | 0,
                0, // according to usbdevice.py this is ignored, so passing in 0
                1); // length

            byte[] writeBuffer = { 0 };

            int numBytesTransferred = 0;

            bool sendResult = usbDevice.ControlTransfer(ref packet, value, 1, out numBytesTransferred);

            return sendResult & (numBytesTransferred > 0);
        }


    }
}