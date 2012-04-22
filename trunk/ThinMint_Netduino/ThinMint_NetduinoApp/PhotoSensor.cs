using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Threading;
using SecretLabs.NETMF.Hardware;

namespace ThinMint_Netduino
{
    class PhotoSensor
    {
        private static OutputPort _input;
        private static int _rotations = 0;
        private static float _rpm = 0;
        private static Boolean _state = false;
        private static Boolean _prevState = false;

        /// <summary>
        /// Pin reading input from the magnetic sensor.
        /// </summary>
        public static OutputPort Input
        {
            get { return PhotoSensor._input; }
            set { PhotoSensor._input = value; }
        }

        /// <summary>
        /// Get the state of the sensor.
        /// </summary>
        public static Boolean GetState()
        {
            return PhotoSensor._state;
        }

        /// <summary>
        /// Get the RPM value.
        /// </summary>
        public static float GetRPM()
        {
            return PhotoSensor._rpm;
        }

        public static void Launch()
        {
            _input = new OutputPort(Pins.GPIO_PIN_D11, false);
            Thread photoThread = (new Thread(new ThreadStart(Listen)));
            photoThread.Start();
        }

        private static void Listen()
        {
            while (true)
            {
                _state = _input.Read();
                if (_state && !_prevState)
                {
                    _rotations++;
                    CalculateRPM();
                }
               // Debug.Print("Photo: " + _state + " . . . RPM: " + _rpm);
                _prevState = _state;
            }
        }

        private static void CalculateRPM()
        {
            _rpm = _rotations;
        }
    }
}
