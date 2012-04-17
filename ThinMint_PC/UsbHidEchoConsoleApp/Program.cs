/* Custom USB HID Communication Device - Echo Test
 * Copyright (c) Secret Labs LLC. All Rights Reserved.
 * 
 * Licensed under the Apache 2.0 open source license
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace UsbHidEchoConsoleApp
{
    class Program
    {
        const int CHARS_TO_SEND = 2048;

        static Stopwatch _stopwatch;

        static void Main(string[] args)
        {
            string hidPath = String.Empty;

            string[] hidPaths = PnP.GetPathNamesByDeviceInterfaceClassGuid(PnP.GetHidGuid());
            foreach (string devicePathName in hidPaths)
            {
                try
                {
                    UInt16 vendorID;
                    UInt16 productID;
                    PnP.ExtractVendorAndProductIDFromDevicePathName(devicePathName, out vendorID, out productID);
                    if (vendorID == 0x22b1 && productID == 0xfffe)
                    {
                        hidPath = devicePathName;
                        break;
                    }
                }
                catch { }
            }

            if (hidPath != string.Empty)
            {
                byte[] data = new byte[1];
                byte[] dataCompare = new byte[1];
                int bytesRead = 0;

                int packetsPerProgressDot = (int)(CHARS_TO_SEND / 36);

                for (int iAttempt = 0; iAttempt < 10000; iAttempt++)
                //                for (int iAttempt = 0; iAttempt < 20; iAttempt++)
                {
                    Console.WriteLine("\r\n[" + iAttempt.ToString() + "] USB HID Device: " + hidPath);
                    UsbHidHostLiteStream usbHostStream = new UsbHidHostLiteStream(hidPath);

                    Console.WriteLine("Now testing: UsbHidDevice | speed test, before optimization");
                    _stopwatch = Stopwatch.StartNew();
                    for (int i = 0; i < CHARS_TO_SEND; i++)
                    {
                        data[0] = (byte)(i % 256);
                        usbHostStream.Write(data, 0, 1);
                        while (true)
                        {
                            bytesRead = usbHostStream.Read(dataCompare, 0, 1);
                            if (dataCompare[0] == data[0])
                                break;
                        }
                        if (i % packetsPerProgressDot == 0)
                            Console.Write("."); // show progress every few bytes
                    }

                    long totalTimeMs = _stopwatch.ElapsedMilliseconds;
                    Console.WriteLine("\r\nPACKETS PER SECOND = " + ((double)CHARS_TO_SEND / ((double)totalTimeMs / (double)1000)));
                    Console.WriteLine("     MS PER PACKET = " + ((double)1000 / ((double)CHARS_TO_SEND / ((double)totalTimeMs / (double)1000))));
                    usbHostStream.Dispose();
                }
            }
            else
            {
                Console.WriteLine("Netduino running custom USB HID code was not found.\r\nTry unplugging and re-attaching USB connect to Netduino and using AC power adapter.\r\n");
            }

            Console.WriteLine("Finished. Press enter to exit.");
            Console.ReadLine();
        }
    }
}
