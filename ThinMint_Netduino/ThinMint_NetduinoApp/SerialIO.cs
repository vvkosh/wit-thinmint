using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Microsoft.SPOT.Messaging;
using System.Threading;
using System.IO.Ports;
using System.Text;

namespace ThinMint_Netduino
{
    class SerialIO
    {
        static SerialPort serial;

        public static void Launch()
        {
            // initialize the serial port for COM1 (using D0 & D1)
            serial = new SerialPort(SerialPorts.COM1, 9600, Parity.None, 8, StopBits.One);
            // open the serial-port, so we can send & receive data
            serial.Open();
            // add an event-handler for handling incoming data
            serial.DataReceived += new SerialDataReceivedEventHandler(serial_DataReceived);
            serial.ErrorReceived += new SerialErrorReceivedEventHandler(serial_ErrorReceived);

            // wait forever...
            Thread.Sleep(Timeout.Infinite);
        }

        static void serial_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Debug.Print("Error:" + e.ToString());
        }

        static void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // wait a little for the buffer to fill
            System.Threading.Thread.Sleep(100);

            // create an array for the incoming bytes
            byte[] bytes = new byte[serial.BytesToRead];
            // read the bytes
            serial.Read(bytes, 0, bytes.Length);
            // convert the bytes into a string
            String line = new String(Encoding.UTF8.GetChars(bytes));

            // write the received bytes, as a string, to the console
            Debug.Print("Data Received: " + line);
            serial.Write(bytes, 0, bytes.Length);
        }
    }
}