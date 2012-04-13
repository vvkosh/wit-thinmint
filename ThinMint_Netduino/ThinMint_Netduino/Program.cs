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
        public static void Main()
        {
            OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
            while (true)
            {
                Thread.Sleep(1000);
                led.Write(!led.Read());
            }
        }

    }
}
