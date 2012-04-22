using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;

namespace ThinMint_Netduino
{
    class LED
    {
        private static OutputPort _led = new OutputPort(Pins.ONBOARD_LED, false);

        public static void On()
        {
            _led.Write(true);
        }

        public static void Off()
        {
            _led.Write(false);
        }
    }
}
