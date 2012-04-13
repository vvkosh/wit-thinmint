using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Threading;

namespace ThinMint_Netduino
{
    class MagneticSensor
    {
        private static OutputPort _input;
        private static OutputPort _power;

        /// <summary>
        /// Pin reading input from the magnetic sensor.
        /// </summary>
        public static OutputPort Input
        {
            get { return MagneticSensor._input; }
            set { MagneticSensor._input = value; }
        }

        /// <summary>
        /// Pin providing power to the magnetic sensor.
        /// </summary>
        public static OutputPort Power
        {
            get { return MagneticSensor._power; }
            set { MagneticSensor._power = value; }
        }

        public static void Launch()
        {
            _input = new OutputPort(Pins.GPIO_PIN_D0, false);
            _power = new OutputPort(Pins.GPIO_PIN_D1, false);
            (new Thread(new ThreadStart(Listen))).Start();
        }

        private static void Listen()
        {
            while (true)
            {
                Debug.Print("Mag: " + _input.Read());
            }
        }
    }
}
