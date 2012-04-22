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
        private static float _inputVoltage = 0;
        private static float _outputVoltage = 0;
        private static float _psi = 0;

        private const float _pMin = 0;
        private const float _pMax = 100;

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

        /// <summary>
        /// PSI reading of the sensor.
        /// </summary>
        public static float PSI
        {
            get { return PressureSensor._psi; }
            set { PressureSensor._psi = value; }
        }

        public static void Launch()
        {
            _input = new OutputPort(Pins.GPIO_PIN_D12, false);
            _output = new OutputPort(Pins.GPIO_PIN_D13, false);
            Thread pressureThread = (new Thread(new ThreadStart(Listen)));
            pressureThread.Start();
        }

        private static void Listen()
        {
            while (true)
            {
                CalculateVoltage();
              //  Debug.Print("Pressure: " + PSI);
            }
        }

        private static void CalculateVoltage()
        {
            PSI = (0.80f / (_pMax - _pMin)) * (_inputVoltage - _pMin) + 0.10f;
        }
    }
}
