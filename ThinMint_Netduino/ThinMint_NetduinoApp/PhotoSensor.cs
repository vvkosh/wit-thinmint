using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Threading;
using SecretLabs.NETMF.Hardware;
using System.Diagnostics;
namespace ThinMint_Netduino
{
    class PhotoSensor
    {
        private static InputPort _input;
        private static int _rotations = 0;
        private static float _rpm = 0;
        private static Boolean _state = false;
        private static Boolean _prevState = false;
        private static Stopwatch timer = Stopwatch.StartNew();
        /// <summary>
        /// Pin reading input from the magnetic sensor.
        /// </summary>
        public static InputPort Input
        {
            get { return PhotoSensor._input; }
            set { PhotoSensor._input = value; }
        }

        /// <summary>
        /// Get the state of the sensor.
        /// </summary>
        public static Boolean State
        {
            get { return PhotoSensor._state; }
        }

        /// <summary>
        /// Get the RPM value.
        /// </summary>
        public static float RPM
        {
            get { return PhotoSensor._rpm; }
        }

        public static void Launch()
        {
            timer.Reset();
            _input = new InputPort(Pins.GPIO_PIN_D9, false, Port.ResistorMode.Disabled);
            Thread photoThread = (new Thread(new ThreadStart(Listen)));
            photoThread.Start();
        }

        private static void Listen()
        {
            timer.Start();
            while (true)
            {
                _state = _input.Read();
                if (_state && !_prevState)
                {
                    _rotations++;
                    if (timer.ElapsedMilliseconds > 0)
                    {
                        CalculateRPM(timer.ElapsedMilliseconds);
                    }
                }
               // Debug.Print("Photo: " + _state + " . . . RPM: " + _rpm);
                _prevState = _state;
            }
            timer.Stop();
        }

        private static void CalculateRPM(long elapsedMilliseconds)
        {
            _rpm = _rotations / elapsedMilliseconds;
        }
    }
}
