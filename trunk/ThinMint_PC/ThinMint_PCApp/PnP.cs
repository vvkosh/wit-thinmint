/* PnP (Plug and Play) Support Class and PnP attachment/detachment event class
 * Copyright (c) Secret Labs LLC. All Rights Reserved.
 * 
 * Licensed under the Apache 2.0 open source license
 * 
 * DESCRIPTION: This class provides functions to support use of the Plug and Play system within Windows.  In particular, it allows the caller to obtain device interface GUIDs
 *              for various device classes (HID to start with), find device paths by class interface, and register/deregister for device attachment/removal WndProc events.
 */

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ThinMint_PCApp
{
    public static class PnP
    {
        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVICE_INTERFACE_DATA
        {
            internal int cbSize;
            internal Guid InterfaceClassGuid;
            internal int Flags;
            internal IntPtr Reserved;
        }

        /* NOTE: this structure is included here only as a description of the data; we manually parse the structure from a buffer in our code
        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            internal int cbSize;
            string DevicePath;
        }
        */

        [DllImport("hid.dll", SetLastError = false)]
        static extern void HidD_GetHidGuid(
            ref Guid HidGuid);

        [DllImport("setupapi.dll", SetLastError = false)]
        static extern int SetupDiDestroyDeviceInfoList(
            IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = false)]
        static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr DeviceInfoSet,
            IntPtr DeviceInfoData,
            ref Guid InterfaceClassGuid,
            int MemberIndex,
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = false, CharSet = CharSet.Auto)]
        static extern IntPtr SetupDiGetClassDevs(
            ref Guid ClassGuid,
            IntPtr Enumerator,
            IntPtr hwndParent,
            Int32 Flags);

        [DllImport("setupapi.dll", SetLastError = false, CharSet = CharSet.Auto)]
        static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr DeviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
            IntPtr DeviceInterfaceDetailData,
            int DeviceInterfaceDetailDataSize,
            ref int RequiredSize,
            IntPtr DeviceInfoData);

        const int DIGCF_PRESENT = 2;
        const int DIGCF_DEVICEINTERFACE = 0x10;

        const long INVALID_HANDLE_VALUE = -1;

        /* Guid HidGuid()
         * DESC: This function returns the HID class's device interface GUID
         * NOTE: The returned GUID is generally used with the GetPathNamesByDeviceInterfaceClassGuid(...) function to obtain paths for all attached HID devices. */
        public static Guid GetHidGuid()
        {
            Guid hidGuid = Guid.Empty;
            HidD_GetHidGuid(ref hidGuid);
            return hidGuid;
        }

        /* string[] GetPathNamesByDeviceInterfaceClassGuid(Guid)
         * DESC: This function returns a string array containing device path names of all attached devices with the device interface class GUID specfified by the 
         *       deviceInterfaceClassGuid parameter) */
        public static string[] GetPathNamesByDeviceInterfaceClassGuid(Guid deviceInterfaceClassGuid)
        {
            // verify that our device interface class guid is not null or empty
            if (deviceInterfaceClassGuid == null)
                throw new ArgumentNullException("deviceInterfaceClassGuid");
            if (deviceInterfaceClassGuid == Guid.Empty)
                throw new ArgumentOutOfRangeException("deviceInterfaceClassGuid");

            // create an array to hold our device path names
            System.Collections.Generic.List<String> devicePathNames = new List<string> { };

            // get a handle to a device information set containing information about all attached devices (of the device interface class specified)
            IntPtr deviceInfoSet = SetupDiGetClassDevs(ref deviceInterfaceClassGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
            if (deviceInfoSet.ToInt64() != INVALID_HANDLE_VALUE)
            {
                try
                {
                    // iterate through all devices in the class, extracting the device path for each one.
                    int memberIndex = 0;
                    bool deviceInterfaceDataAvailable = true;
                    while (deviceInterfaceDataAvailable)
                    {
                        SP_DEVICE_INTERFACE_DATA deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
                        deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);

                        // see if there is a(nother) device in this set; if so, the following function will return success
                        deviceInterfaceDataAvailable = SetupDiEnumDeviceInterfaces(deviceInfoSet, IntPtr.Zero, ref deviceInterfaceClassGuid, memberIndex, ref deviceInterfaceData);
                        if (deviceInterfaceDataAvailable)
                        {
                            // we successfully obtained DeviceInterfaceData for this device; extract the device path name

                            // first, retrieve SP_DEVICE_INTERFACE_DETAIL_DATA for the device
                            int deviceInterfaceDetailDataBufferSize = 0;
                            IntPtr deviceInterfaceDetailDataBufferPtr;

                            // call SetupDiGetDeviceInterfaceDetail a first time to get the required size for the detail buffer...
                            SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, IntPtr.Zero, 0, ref deviceInterfaceDetailDataBufferSize, IntPtr.Zero);
                            // allocate a detail buffer manually instead of using the SP_DEVICE_INTERFACE_DETAIL_DATA structure (since it has layout troubles with a variable-length string).
                            deviceInterfaceDetailDataBufferPtr = Marshal.AllocHGlobal(deviceInterfaceDetailDataBufferSize);
                            /* fill the first 4 bytes (cbSize) with the size of the buffer (in this case, the size of cbSize plus the size of a null character; please note that
                             * we detect a 64-bit system here and use a size of 8 in that case as the structure will be formatted/sized accordingly. */
                            if (IntPtr.Size == 8)
                                Marshal.WriteInt32(deviceInterfaceDetailDataBufferPtr, 8);
                            else
                                Marshal.WriteInt32(deviceInterfaceDetailDataBufferPtr, 4 + Marshal.SystemDefaultCharSize);


                            // then, call SetupDiGetDeviceInterfaceDetail again with a proper detail buffer so we can extract the device path name
                            if (SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref deviceInterfaceData, deviceInterfaceDetailDataBufferPtr,
                                deviceInterfaceDetailDataBufferSize, ref deviceInterfaceDetailDataBufferSize, IntPtr.Zero))
                            {
                                // success obtaining the device interface detail buffer (i.e. device path name); extract our device path name (4 bytes into the buffer)
                                string devicePathName = Marshal.PtrToStringAuto(new IntPtr(deviceInterfaceDetailDataBufferPtr.ToInt32() + 4));
                                // add the device path name to our array...
                                devicePathNames.Add(devicePathName);
                            }
                            // clean up our detail buffer (whether or not we succeeded)...
                            Marshal.FreeHGlobal(deviceInterfaceDetailDataBufferPtr);
                        }

                        memberIndex++;
                    }
                }
                finally
                {
                    // clean up our array of device info structures
                    SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }
            }

            // return our list of device path names (or an empty array if we couldn't/didn't find any)
            return devicePathNames.ToArray();
        }

        /* void ExtractVidPidFromDevicePathName(string, out UInt16, out UInt16)
         * DESC: This function extracts the vendor id (VID) and product id (PID) from a device path name */
        public static void ExtractVendorAndProductIDFromDevicePathName(string devicePathName, out UInt16 vendorID, out UInt16 productID)
        {
            if (devicePathName == null)
                throw new ArgumentNullException(devicePathName);

            // extract the vendor id from our pathname...
            if (devicePathName.ToUpper().IndexOf("VID_") != -1)
            {
                int indexOfVid = devicePathName.ToUpper().IndexOf("VID_") + 4;
                int lengthOfVid = devicePathName.IndexOf("&", indexOfVid + 1) - indexOfVid;
                if (indexOfVid > 0 && lengthOfVid == 4)
                    vendorID = Convert.ToUInt16(devicePathName.Substring(indexOfVid, lengthOfVid), 16);
                else
                    throw new ArgumentException("devicePathName");

                // extract the product id from our pathname...
                if (devicePathName.ToUpper().IndexOf("PID_") != -1)
                {
                    int indexOfPid = devicePathName.ToUpper().IndexOf("PID_") + 4;
                    int lengthOfPid = 4;
                    if (indexOfPid > 0 && indexOfPid == indexOfVid + lengthOfVid + 5)
                        productID = Convert.ToUInt16(devicePathName.Substring(indexOfPid, lengthOfPid), 16);
                    else
                        throw new ArgumentException("devicePathName");
                }
                else
                {
                    throw new ArgumentException("devicePathName");
                }
            }
            else
            {
                throw new ArgumentException("devicePathName");
            }
        }

        /* class DeviceEventNotifier
         * DESC: This class allows a window or service to register for plug and play events, interprets those events and raises events accordingly. */
        public class DeviceEventNotifier : IDisposable
        {
            const int DBT_DEVTYP_DEVICEINTERFACE = 0x5;

            const int DBT_DEVICEARRIVAL = 0x8000;
            const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

            const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;
            const int DEVICE_NOTIFY_SERVICE_HANDLE = 1;

            const int DIGCF_PRESENT = 2;
            const int DIGCF_DEVICEINTERFACE = 0x10;

            const int WM_DEVICECHANGE = 0x219;

            [StructLayout(LayoutKind.Sequential)]
            class DEV_BROADCAST_DEVICEINTERFACE
            {
                internal int dbcc_size;
                internal int dbcc_devicetype;
                internal int dbcc_reserved;
                internal Guid dbcc_classguid;
                internal Char dbcc_name; // NOTE: this represents a one-character array
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            class DEV_BROADCAST_DEVICEINTERFACE_CUSTOM_1 // this is an alternate declaration of DEV_BROADCAST_DEVICEINTERFACE we are creating for convenience
            {
                internal int dbcc_size;
                internal int dbcc_devicetype;
                internal int dbcc_reserved;
                //[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)] // NOTE: we could also use this to represent GUID as a byte array...
                //internal byte[] dbcc_classguid;
                internal Guid dbcc_classguid;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
                internal Char[] dbcc_name;
            }

            [StructLayout(LayoutKind.Sequential)]
            class DEV_BROADCAST_HDR
            {
                internal int dbch_size;
                internal int dbch_devicetype;
                internal int dbch_reserved;
            }

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
            static extern IntPtr RegisterDeviceNotification(
                IntPtr hRecipient,
                IntPtr NotificationFilter,
                int Flags);

            [DllImport("user32.dll", SetLastError = false)]
            static extern IntPtr UnregisterDeviceNotification(
                IntPtr Handle);

            public class DeviceAttachmentEventArgs : EventArgs
            {
                public readonly string DevicePathName;

                internal DeviceAttachmentEventArgs(string devicePathName)
                {
                    DevicePathName = devicePathName;
                }
            }

            public delegate void DeviceAttachmentEventHandler(object sender, DeviceAttachmentEventArgs e);
            public event DeviceAttachmentEventHandler DeviceAttached;
            public event DeviceAttachmentEventHandler DeviceRemoved;

            private IntPtr m_deviceNotificationHandle = IntPtr.Zero;

            private bool m_deviceNotificationIsRegistered = false;

            ListenerMessageWindow m_listenerMessageWindow = null;

            /* DeviceEventNotifier(IntPtr, deviceInterfaceGuid)
             * DESC: This function takes a service handle and deviceInterfaceGuid as parameters.  This is an overloaded constructor which allows compatibility with services.
             * NOTE: Unlike standard applications, Windows Services must incorporate extra code to allow this class to receive desired plug and play events.
             * 
             * TEMPLATE FOR WINDOWS SERVICES CODE:
             * public partial class Service1 : ServiceBase
             * {
             *     [DllImport("advapi32.dll", SetLastError = true)]
             *     public static extern IntPtr RegisterServiceCtrlHandlerEx(string lpServiceName, ServiceCtrlHandlerEx lpHandlerProc, IntPtr lpContext);
             * 
             *     public delegate int ServiceCtrlHandlerEx(int control, int eventType, IntPtr eventData, IntPtr context);
             *     ServiceCtrlHandlerEx myServiceCtrlHandler;
             * 
             *     public const int SERVICE_CONTROL_STOP = 1;
             *     public const int SERVICE_CONTROL_SHUTDOWN = 5;
             *     public const int SERVICE_CONTROL_DEVICEEVENT = 11;
             *     
             *     protected override void OnStart(string[] args)
             *     {
             *         // before we can create our DeviceEventNotifier, we must register this service to receive messages--and create a handler which can parse them and pass them along...
             *         myServiceCtrlHandler = new ServiceCtrlHandlerEx(ServiceControlHandler);
             *         IntPtr statusHandle = RegisterServiceCtrlHandlerEx(this.ServiceName, myServiceCtrlHandler, IntPtr.Zero); // NOTE: does not require later de-registation
             *         if (statusHandle.ToInt32() == 0)
             *         {
             *             // error: no handle returned
             *             // CODE GOES HERE: handle this error
             *         }
             *         
             *         // CODE GOES HERE: create DeviceEventNotifier object, passing in this.ServiceHandle and the HID or another GUID
             *         // CODE GOES HERE: hook up DeviceEventNotifier.DeviceAttached and DeviceEventNotifier.DeviceRemoved events
             *       
             *         // CODE GOES HERE: other OnStart() tasks
             *     }
             *     
             *     private int ServiceControlHandler(int control, int eventType, IntPtr eventData, IntPtr context)
             *     {
             *         if (control == SERVICE_CONTROL_STOP || control == SERVICE_CONTROL_SHUTDOWN)
             *         {
             *             base.Stop();
             *         }
             *         else if (control == SERVICE_CONTROL_DEVICEEVENT)
             *         {
             *             if (m_DeviceEventNotifier != null)
             *                 m_DeviceEventNotifier.ProcessDeviceEvent(eventType, eventData);
             *         }
             *     
             *         return 0; // NO_ERROR == 0
             *     }
             *     
             *     protected override void OnStop()
             *     {
             *         // dispose our DeviceEventNotifier (which will unregister PnP events for the HID guid)...
             *         m_DeviceEventNotifier.Dispose();
             *         
             *         // CODE GOES HERE: other OnStop() tasks
             *     }
             * }
             * */
            public DeviceEventNotifier(IntPtr serviceHandle, Guid deviceInterfaceGuid)
            {
                // make sure that we have a service handle
                if (serviceHandle == IntPtr.Zero)
                    throw new ArgumentException("serviceHandle must not be zero.", "serviceHandle");

                // register service handle for PnP notifications
                DEV_BROADCAST_DEVICEINTERFACE devBroadcastDeviceInterface = new DEV_BROADCAST_DEVICEINTERFACE();
                devBroadcastDeviceInterface.dbcc_size = Marshal.SizeOf(devBroadcastDeviceInterface);
                devBroadcastDeviceInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
                devBroadcastDeviceInterface.dbcc_reserved = 0;
                devBroadcastDeviceInterface.dbcc_classguid = deviceInterfaceGuid;

                IntPtr devBroadcastDeviceInterfaceBufferPtr = Marshal.AllocHGlobal(devBroadcastDeviceInterface.dbcc_size);
                Marshal.StructureToPtr(devBroadcastDeviceInterface, devBroadcastDeviceInterfaceBufferPtr, true);
                m_deviceNotificationHandle = RegisterDeviceNotification(serviceHandle, devBroadcastDeviceInterfaceBufferPtr, DEVICE_NOTIFY_SERVICE_HANDLE);
                Marshal.FreeHGlobal(devBroadcastDeviceInterfaceBufferPtr);

                m_deviceNotificationIsRegistered = true;
            }

            /* DeviceEventNotifier(deviceInterfaceGuid)
             * DESC: This function registers an invisible window to receive plug-and-play notifications for deviceInterfaceGuid. */
            public DeviceEventNotifier(Guid deviceInterfaceGuid)
            {
                // create window listener for PnP notifications
                m_listenerMessageWindow = new ListenerMessageWindow();
                m_listenerMessageWindow.OnMessageReceived += new ListenerMessageWindow.OnMessageReceivedEventHandler(m_listenerMessageWindow_OnMessageReceived);

                // register window handle for PnP notifications
                DEV_BROADCAST_DEVICEINTERFACE devBroadcastDeviceInterface = new DEV_BROADCAST_DEVICEINTERFACE();
                devBroadcastDeviceInterface.dbcc_size = Marshal.SizeOf(devBroadcastDeviceInterface);
                devBroadcastDeviceInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
                devBroadcastDeviceInterface.dbcc_reserved = 0;
                devBroadcastDeviceInterface.dbcc_classguid = deviceInterfaceGuid;

                IntPtr devBroadcastDeviceInterfaceBufferPtr = Marshal.AllocHGlobal(devBroadcastDeviceInterface.dbcc_size);
                Marshal.StructureToPtr(devBroadcastDeviceInterface, devBroadcastDeviceInterfaceBufferPtr, true);
                m_deviceNotificationHandle = RegisterDeviceNotification(m_listenerMessageWindow.Handle, devBroadcastDeviceInterfaceBufferPtr, DEVICE_NOTIFY_WINDOW_HANDLE);
                Marshal.FreeHGlobal(devBroadcastDeviceInterfaceBufferPtr);

                m_deviceNotificationIsRegistered = true;
            }

            void m_listenerMessageWindow_OnMessageReceived(object sender, ListenerMessageWindow.OnMessageReceivedEventArgs e)
            {
                if (e.Message.Msg == WM_DEVICECHANGE)
                    ProcessDeviceEvent(e.Message.WParam.ToInt32(), e.Message.LParam);
            }

            /* UnregisterAll()
             * DESC: This function unregisters our service or invisible window; we will no longer receive plug-and-play notifications. */
            public void UnregisterAll()
            {
                if (m_deviceNotificationIsRegistered)
                {
                    if (m_deviceNotificationHandle != IntPtr.Zero)
                        UnregisterDeviceNotification(m_deviceNotificationHandle);
                }
                m_deviceNotificationIsRegistered = false;
            }

            /* void ProcessDeviceEvent(Message m)
             * DESC: This function processes a WndProc message and, if appropriate, raises the DeviceAttached or DeviceRemoved event. */
            public void ProcessDeviceEvent(int eventType, IntPtr eventData)
            {
                // device change event; see if this is our device attachment/removal...
                if (eventType == DBT_DEVICEARRIVAL || eventType == DBT_DEVICEREMOVECOMPLETE)
                {
                    DEV_BROADCAST_HDR devBroadcastHdr = new DEV_BROADCAST_HDR();
                    Marshal.PtrToStructure(eventData, devBroadcastHdr);

                    if (devBroadcastHdr.dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
                    {
                        DEV_BROADCAST_DEVICEINTERFACE_CUSTOM_1 devBroadcastDeviceInterface = new DEV_BROADCAST_DEVICEINTERFACE_CUSTOM_1();

                        // get the size of the actual string
                        int size = Convert.ToInt32((devBroadcastHdr.dbch_size - 32) / 2);
                        Array.Resize(ref devBroadcastDeviceInterface.dbcc_name, size);
                        // get the device interface structure
                        Marshal.PtrToStructure(eventData, devBroadcastDeviceInterface);
                        // get the device path name
                        string devicePathName = new string(devBroadcastDeviceInterface.dbcc_name, 0, size);

                        switch (eventType)
                        {
                            case DBT_DEVICEARRIVAL:
                                DeviceAttached(this, new DeviceAttachmentEventArgs(devicePathName));
                                break;
                            case DBT_DEVICEREMOVECOMPLETE:
                                DeviceRemoved(this, new DeviceAttachmentEventArgs(devicePathName));
                                break;
                        }
                    }
                }
            }

            public class ListenerMessageWindow : Form
            {
                public class OnMessageReceivedEventArgs : EventArgs
                {
                    public Message Message;

                    internal OnMessageReceivedEventArgs(Message message)
                    {
                        this.Message = message;
                    }
                }
                public delegate void OnMessageReceivedEventHandler(object sender, OnMessageReceivedEventArgs e);
                public event OnMessageReceivedEventHandler OnMessageReceived;

                protected override void WndProc(ref Message msg)
                {
                    OnMessageReceived(this, new OnMessageReceivedEventArgs(msg));

                    // call the base class WndProc for default message handling
                    base.WndProc(ref msg);
                }
            }

            #region IDisposable Members

            public void Dispose()
            {
                // if we have not unregistered for PnP notifications, do so now.
                if (m_deviceNotificationIsRegistered)
                    UnregisterAll();
                if (m_listenerMessageWindow != null)
                    m_listenerMessageWindow.Dispose();
            }

            #endregion
        }
    }
}
