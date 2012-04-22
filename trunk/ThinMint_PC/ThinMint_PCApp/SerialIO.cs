using System.IO.Ports;
using System;
using System.Diagnostics;
using System.Text;

namespace ThinMint_PC
{
    class SerialIO
    {
        private static SerialPort _serial;

        public static SerialPort Serial
        {
            get { return SerialIO._serial; }
            set { SerialIO._serial = value; }
        }

        public static void Initialize()
        {
            // provide some usage information
            System.Console.WriteLine("enter some text and hit ENTER.");
            System.Console.WriteLine("enter 'x' and hit ENTER to exit.");
            System.Console.WriteLine();

            OpenSerialPort("COM6");

            // this will hold each line entered
            String line = string.Empty;

            // as long as an x is not entered
            while (line.ToLowerInvariant() != "x")
            {
                // read a single line from the console
                line = System.Console.ReadLine();
                SendData(line);
            }
        }

        /// <summary>
        /// Opens a serial port and attaches a listener.
        /// </summary>
        /// <param name="serial">ID of the serial port (COM1, COM2, COM3, etc.)</param>
        public static void OpenSerialPort(String serial)
        {
            Serial = new SerialPort(serial, 9600, Parity.None, 8, StopBits.One);            // BC - Initialize the serial port.
            Serial.Open();
            Serial.DataReceived += new SerialDataReceivedEventHandler(serial_DataReceived); // BC - Attach an event listener.
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
            else if (header == 'M') // BC - Basic message.
            {
                Console.Out.WriteLine(message);
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

        private static void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            System.Threading.Thread.Sleep(100);                         // BC - Pause a moment to let the buffer fill.
            byte[] bytes = new byte[Serial.BytesToRead];                // BC - Initialize a byte array.
            Serial.Read(bytes, 0, bytes.Length);                        // BC - Read in the bytes.
            if (bytes.Length > 0)                                       // BC - Disregard empty messages.
            {
                String line = Encoding.UTF8.GetString(bytes);           // BC - Convert the bytes to a string.
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
    }
}