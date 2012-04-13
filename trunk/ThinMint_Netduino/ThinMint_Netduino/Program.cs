using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace ThinMint_Netduino
{
    public class Program
    {
        private static OutputPort _led = new OutputPort(Pins.ONBOARD_LED, false);

        public static void Main()
        {
            MagneticSensor.Launch();
            PressureSensor.Launch();

            while (true)
            {
                Thread.Sleep(1000);
                _led.Write(true);
                Thread.Sleep(1000);
                _led.Write(false);
            }
        }

    }
}
