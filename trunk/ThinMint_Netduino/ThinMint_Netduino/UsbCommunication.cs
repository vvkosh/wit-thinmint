/* Custom USB HID Communication Device - Echo Device
 * Copyright (c) Secret Labs LLC. All Rights Reserved.
 * 
 * Licensed under the Apache 2.0 open source license
 */

using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.UsbClient;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace ThinMint_Netduino
{
    class UsbCommunication
    {
        const int USBSTREAM_WRITE_ENDPOINT = 1;
        const int USBSTREAM_READ_ENDPOINT = 2;
        const int MAXPOWER_MILLIAMPS = 280; // default; can be up to 500mA

        const int HID_DESCRIPTOR_TYPE = 0x21;

        public static void Launch()
        {
            // write your code here
            // Get a reference to our main USB controller
            UsbController[] usbControllers = UsbController.GetControllers();
            if (usbControllers.Length < 1)
                return; // if we don't have any USB controllers, abort
            UsbController usbController = usbControllers[0];

#if DEBUG
            // make sure that our USB controller is not already running (i.e. being used for debugging)
            if (usbController.Status == UsbController.PortState.Running)
                return; // in use; abort
#endif

            // configure our USB controller, start it running, and create a stream for our endpoint (endpoint #1)
            UsbStream usbStream = null;
            try
            {
                // configure the controller
                ConfigureUsbController(usbController);
                // start up our controller
                if (!usbController.Start())
                    return; // could not start usb controller; abort
                // finally, create our usb stream -- this must be done before we attach a USB cable between our Netduino and host/computer
                usbStream = usbController.CreateUsbStream(USBSTREAM_WRITE_ENDPOINT, USBSTREAM_READ_ENDPOINT);
            }
            catch
            {
                // cannot configure USB controller; abort
                return;
            }

            // uncomment the following code to force device to enumerate (i.e. force USB cable to be reattached) before starting echo logic
            /*
            Debug.Print("\r\nPlease disconnect and reconnect USB cable to start communication with device.");
            UsbController.PortState usbPortState;
            while (true)
            {
                usbPortState = usbController.Status;
                Debug.Print("status: " + PortStateToString(usbPortState));

                if (usbPortState == UsbController.PortState.Running)
                    break;

                System.Threading.Thread.Sleep(1000);
            }
            Debug.Print("USB is now attached.");
            */

            // start our echo logic (for now, our only thread)
            (new Thread(new ThreadStart(InfiniteEchoLoop(usbController, usbStream)))).Start();
        }

        static string PortStateToString(Microsoft.SPOT.Hardware.UsbClient.UsbController.PortState portState)
        {
            switch (portState)
            {
                case UsbController.PortState.Address:
                    return "Address";
                case UsbController.PortState.Attached:
                    return "Attached";
                case UsbController.PortState.Default:
                    return "Default";
                case UsbController.PortState.Detached:
                    return "Detached";
                case UsbController.PortState.Powered:
                    return "Powered";
                case UsbController.PortState.Running:
                    return "Running";
                case UsbController.PortState.Stopped:
                    return "Stopped";
                case UsbController.PortState.Suspended:
                    return "Suspended";
                default:
                    return "";
            }
        }

        static void ConfigureUsbController(UsbController usbController)
        {
            // setup our USB Controller's configuration
            Configuration configuration = new Configuration();

            // device descriptor
            Configuration.DeviceDescriptor deviceDescriptor = new Configuration.DeviceDescriptor(0x22B1, 0xFFFE, 0x0100); // VID, PID, version
            deviceDescriptor.bcdUSB = 0x0200;
            deviceDescriptor.bDeviceClass = 0x00;
            deviceDescriptor.bDeviceSubClass = 0x00;
            deviceDescriptor.bDeviceProtocol = 0x00;
            deviceDescriptor.bMaxPacketSize0 = 0x08;
            deviceDescriptor.iManufacturer = 0x01;   // string 1: manufacturer name
            deviceDescriptor.iProduct = 0x02;        // string 2: product name
            deviceDescriptor.iSerialNumber = 0x03;   // string 3: serial number

            // interface descriptor
            // HID class descriptor payload
            byte[] hidClassDescriptorPayload = new byte[] 
            {
                0x01, 0x01,     // bcdHID (v1.01)
                0x00,           // bCountryCode
                0x01,           // bNumDescriptors
                0x22,           // bDescriptorType (report)
                0x22, 0x00      // wDescriptorLength (report descriptor size in bytes)
            };
            // HID class descriptor
            Configuration.ClassDescriptor hidClassDescriptor = new Configuration.ClassDescriptor(HID_DESCRIPTOR_TYPE, hidClassDescriptorPayload);
            // write endpoint
            Configuration.Endpoint writeEndpoint = new Configuration.Endpoint(USBSTREAM_WRITE_ENDPOINT, Configuration.Endpoint.ATTRIB_Interrupt | Configuration.Endpoint.ATTRIB_Write);
            writeEndpoint.wMaxPacketSize = 64;   // packet size: 64 bytes
            writeEndpoint.bInterval = 1;         // interval: 1ms
            // read endpoint
            Configuration.Endpoint readEndpoint = new Configuration.Endpoint(USBSTREAM_READ_ENDPOINT, Configuration.Endpoint.ATTRIB_Interrupt | Configuration.Endpoint.ATTRIB_Read);
            readEndpoint.wMaxPacketSize = 64;   // packet size: 64 bytes
            readEndpoint.bInterval = 1;         // interval: 1ms
            //
            Configuration.UsbInterface usbInterface = new Configuration.UsbInterface(0, new Configuration.Endpoint[] { writeEndpoint, readEndpoint });
            usbInterface.classDescriptors = new Configuration.ClassDescriptor[] { hidClassDescriptor };
            usbInterface.bInterfaceClass = 0x03;    // HID
            usbInterface.bInterfaceSubClass = 0x00;
            usbInterface.bInterfaceProtocol = 0x00;

            // configuration descriptor
            Configuration.ConfigurationDescriptor configurationDescriptor = new Configuration.ConfigurationDescriptor(MAXPOWER_MILLIAMPS, new Configuration.UsbInterface[] { usbInterface });

            // HID report descriptor
            byte[] hidGenericReportDescriptorPayload = new byte[]
            {
                0x06, 0xA0, 0xFF,   // Usage Page (vendor-defined)
                0x09, 0x01,         // Usage (vendor-defined)

                0xA1, 0x01,         // Collection (Application)

                0x09, 0x03,         // Usage (vendor-defined)
                0x15, 0x00,         // Logical Minimum (0)
                0x26, 0xFF, 0x00,   // Logical Maximum (255)
                0x75, 0x08,         // Report Size (8 bits)
                0x95, 0x40,         // Report Count (64)
                0x81, 0x02,         // Input (Data, Variable, Absolute)

                0x09, 0x04,         // Usage (vendor-defined)
                0x15, 0x00,         // Logical Minimum (0)
                0x26, 0xFF, 0x00,   // Logical Maximum (255)
                0x75, 0x08,         // Report Size (8 bits)
                0x95, 0x40,         // Report Count (64)
                0x91, 0x02,         // Output (Data, Variable, Absolute)

                0xC0                // End Collection
            };
            Configuration.GenericDescriptor hidGenericReportDescriptor = new Configuration.GenericDescriptor(0x81, 0x2200, hidGenericReportDescriptorPayload);
            hidGenericReportDescriptor.bRequest = 0x06; // GET_DESCRIPTOR
            hidGenericReportDescriptor.wIndex = 0x00; // INTERFACE 0 (zero)

            Configuration.StringDescriptor manufacturerNameStringDescriptor = new Configuration.StringDescriptor(1, "Secret Labs LLC");
            Configuration.StringDescriptor productNameStringDescriptor = new Configuration.StringDescriptor(2, "HID Communication Device");
            Configuration.StringDescriptor serialNumberStringDescriptor = new Configuration.StringDescriptor(3, "1");
            Configuration.StringDescriptor displayNameStringDescriptor = new Configuration.StringDescriptor(4, "HID Communication Device");
            Configuration.StringDescriptor friendlyNameStringDescriptor = new Configuration.StringDescriptor(5, "HID Communication Device");

            configuration.descriptors = new Configuration.Descriptor[]
            {
                deviceDescriptor,
                configurationDescriptor, 
                manufacturerNameStringDescriptor, 
                productNameStringDescriptor,
                serialNumberStringDescriptor,
                displayNameStringDescriptor,
                friendlyNameStringDescriptor,
                hidGenericReportDescriptor
            };

            // save the final configuration to our controller
            //usbController.Stop();
            usbController.Configuration = configuration;

            // if we could not configure our usb controller, abort
            if (usbController.ConfigurationError != UsbController.ConfigError.ConfigOK)
                return;
        }



        static void InfiniteEchoLoop(UsbController usbController, UsbStream usbStream)
        {
            int bytesRead;

            byte[] buffer = new byte[100];
            int bufferLength = 100;

            while (true)
            {
                // read data from the usb stream
                bytesRead = usbStream.Read(buffer, 0, bufferLength);
                // echo the data back to the sender
                if (bytesRead > 0)
                {
                    usbStream.Write(buffer, 0, bytesRead);
                }
            } // repeat forever 
        }
    }
}
