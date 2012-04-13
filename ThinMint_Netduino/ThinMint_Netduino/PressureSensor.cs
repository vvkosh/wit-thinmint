using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Threading;
using SecretLabs.NETMF.Hardware;

namespace ThinMint_Netduino
{
    class PressureSensor
    {
        private static OutputPort _input;
        private static OutputPort _output;

        /// <summary>
        /// Pin reading input from the PSI sensor.
        /// </summary>
        public static OutputPort Input
        {
            get { return PressureSensor._input; }
            set { PressureSensor._input = value; }
        }

        /// <summary>
        /// Pin providing power to the PSI sensor.
        /// </summary>
        public static OutputPort Output
        {
            get { return PressureSensor._output; }
            set { PressureSensor._output = value; }
        }

        public static void Launch()
        {
            _input = new OutputPort(Pins.GPIO_PIN_D0, false);
            _output = new OutputPort(Pins.GPIO_PIN_D1, false);
            (new Thread(new ThreadStart(Listen))).Start();
        }

        private static void Listen()
        {
            while (true)
            {
                Debug.Print("Pressure: " + _input.Read());
            }
        }


        private static float CalculateVoltage(float inputVoltage)
        {
            float outputVoltage;
            outputVoltage = 0; // EQUATION GOES HERE
            return outputVoltage;
        }
    }
}
