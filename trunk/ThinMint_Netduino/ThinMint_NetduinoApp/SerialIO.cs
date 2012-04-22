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
        private static SerialPort _serial;

        public static SerialPort Serial
        {
            get { return SerialIO._serial; }
            set { SerialIO._serial = value; }
        }

        public static void Launch()
        {
            // initialize the serial port for COM1 (using D0 & D1)
            OpenSerialPort("COM1");

            // BC - Start up the sending thread.
            Thread sendingThread = new Thread(new ThreadStart(Send));
            sendingThread.Start();

            // wait forever...
            Thread.Sleep(Timeout.Infinite);
        }

        public static void Send()
        {
            while (true)
            {
                Thread.Sleep(200);
                SendData(Encode(ValueType.RPM,PhotoSensor.RPM));
                Thread.Sleep(200);
                SendData(Encode(ValueType.PSI,PressureSensor.PSI));
                Thread.Sleep(200);
                SendData(Encode(ValueType.SLOPE, Calculator.Slope));
            }
        }

        /// <summary>
        /// Opens a serial port and attaches a listener.
        /// </summary>
        /// <param name="serial">ID of the serial port (COM1, COM2, COM3, etc.)</param>
        public static void OpenSerialPort(String serial)
        {
            Serial = new SerialPort(serial, 9600, Parity.None, 8, StopBits.One);               // BC - Initialize the serial port.
            Serial.Open();
            Serial.DataReceived += new SerialDataReceivedEventHandler(serial_DataReceived);    // BC - Attach an event listener.
            Serial.ErrorReceived += new SerialErrorReceivedEventHandler(serial_ErrorReceived); // BC - Attach an error listener.
        }

        /// <summary>
        /// BC - Encodes a message to be sent through the Serial Communicator.
        /// </summary>
        /// <param name="encoding">Type of message this is.</param>
        /// <param name="line">Message to be sent.</param>
        /// <returns>The encoded message.</returns>
        public static String Encode(ValueType encoding, String line)
        {
            String output = "";
            if (encoding == ValueType.PSI)      // BC - PSI value.
            {
                output = "P" + line;
            }
            else if (encoding == ValueType.RPM) // BC - RPM value.
            {
                output = "R" + line;
            }
            else if (encoding == ValueType.SLOPE) // BC - Slope value.
            {
                output = "S" + line;
            }
            else                                // BC - Basic message.
            {
                output = "M" + line;
            }
            return output;
        }

        /// <summary>
        /// BC - Encodes a float to be sent through the serial communicator.
        /// </summary>
        /// <param name="encoding">Type of message this is.</param>
        /// <param name="value">Value to be sent.</param>
        /// <returns>Encoded message.</returns>
        public static String Encode(ValueType encoding, float value)
        {
            return (Encode(encoding, "" + value));   // BC - Convert the float to a string and pass it along.
        }

        /// <summary>
        /// BC - Parses and handles the message.
        /// </summary>
        /// <param name="line">Line to be parsed.</param>
        /// <returns>Parsed message.</returns>
        private static String Parse(String line)
        {
            char header = line[0];
            String message = line.Substring(1);
            if (header == 'P')      // BC - PSI value.
            {

            }
            else if (header == 'R') // BC - RPM value.
            {

            }
            else if (header == 'S') // BC - Slope value.
            {

            }
            else if (header == 'M') // BC - Basic message.
            {
                SendData(Encode(ValueType.MSG,message));
            }
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        public static void SendData(String line)
        {
            Debug.Print("Sending message: [" + line + "]");                 // BC - Print the line through the debugger.
            byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(line);    // BC - Convert the line to bytes.
            Serial.Write(utf8Bytes, 0, utf8Bytes.Length);                   // BC - Send the bytes over the serial port.
        }

        private static void serial_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Debug.Print("Error received: [" + e.ToString() +"]");
        }

        private static void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            LED.On();
            System.Threading.Thread.Sleep(100);                         // BC - Pause a moment to let the buffer fill.
            LED.Off();
            byte[] bytes = new byte[Serial.BytesToRead];                // BC - Initialize a byte array.
            Serial.Read(bytes, 0, bytes.Length);                        // BC - Read in the bytes.
            if (bytes.Length > 0)                                       // BC - Disregard empty messages.
            {
                String line = new String(Encoding.UTF8.GetChars(bytes));// BC - Convert the bytes to a string.
                Debug.Print("Received message: [" + line + "]");        // BC - Print the line through the debugger.
                Parse(line);                                            // BC - Parse the line.
            }
        }
    }

    /// <summary>
    /// Different types of messages that can be sent.
    /// </summary>
    enum ValueType
    {
        MSG,
        RPM,
        PSI,
        SLOPE,
    }
}