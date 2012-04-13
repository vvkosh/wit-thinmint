using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Threading;

namespace ThinMint_Netduino
{
    class PressureSensor
    {
        private static OutputPort _input;
        private static OutputPort _power;

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
        public static OutputPort Power
        {
            get { return PressureSensor._power; }
            set { PressureSensor._power = value; }
        }

        public static void Launch()
        {
            _input = new OutputPort(Pins.GPIO_PIN_A0, false);
            _power = new OutputPort(Pins.GPIO_PIN_A1, false);
            (new Thread(new ThreadStart(Listen))).Start();
        }

        private static void Listen()
        {
            while (true)
            {
                Debug.Print("PSI: " + _input.Read());
            }
        }
    }
}
