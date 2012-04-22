using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using System.Threading;
using SecretLabs.NETMF.Hardware;
using System.Diagnostics;
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
        private const float _outMax = 2104;
        private const float _outMin = 1632;


        private static SPI.Configuration Device1 = new SPI.Configuration(
            Pins.GPIO_PIN_D10, // SS-pin
            false,             // SS-pin active state
            0,                 // The setup time for the SS port
            0,                 // The hold time for the SS port
            true,              // The idle state of the clock
            false,             // The sampling clock edge
            8000,              // The SPI clock rate in KHz
            SPI_Devices.SPI1   // The used SPI bus (refers to a MOSI MISO and SCLK pinset)
        );

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
            //PSI = (0.80f / (_pMax - _pMin)) * (_inputVoltage - _pMin) + 0.10f;
            // Setup the spi for 8 bit data with a 800KHz clock rate 
            SPI SPIBus = new SPI(Device1);
           // spi.format(8, 0);
           // spi.frequency(800000);

            while (true)
            {
                // Select the device by seting chip select low 
                //cs = 0;
                Thread.Sleep(100);
                byte[] WriteBuffer = new byte[1];
                byte[] ReadBuffer = new byte[1];
                // Send a dummy byte to receive the contents
                WriteBuffer[0] = 0x00;
                int byte_1 = spi.write(0x00);
                int byte_2 = spi.write(0x00);
                int byte_3 = spi.write(0x00);
                int byte_4 = spi.write(0x00);

                //float temp = byte_3 << 3;
                //temp = ((temp / 2047) * 200) - 50;
                float psi = byte_1 << 8 | byte_2;
                //psi = byte_1|byte_2; 
                psi = (psi / 13108) - (1743 / 13108);

                //Debug.Print("Byte 1 = %X; Byte 2 = %X; Byte 3 = %X; Byte 4 = %X; PSI =  %.4f; TEMP = %.2f\r", byte_1, byte_2, byte_3, byte_4, psi, temp);

                // Deselect the device 
                //cs = 1;
                Thread.Sleep(5000);
            }
        }
    }
}




//#include "mbed.h" 
//#define OUT_MAX 2104 
//#define OUT_MIN 1632 
//#define PRES_MAX 5 
//#define PRES_MIN 0 
 
//SPI spi(p5, p6, p7); // mosi - no connection, miso - connected to pin 3 of sensor, sclk connected to pin 4 of sensor 
//DigitalOut cs(p8);   // chip Select line connected to pin 5 of sensor 
 
//Serial pc(p13,p14); // tx, rx 
 
//int main() { 
//    // Setup the spi for 8 bit data with a 800KHz clock rate 
//    spi.format(8,0); 
//    spi.frequency(800000); 
 
//    while(1) 
//    { 
//    // Select the device by seting chip select low 
//    cs = 0; 
//    wait(0.1); 
//    // Send a dummy byte to receive the contents 
//    int byte_1 = spi.write(0x00); 
//    int byte_2 = spi.write(0x00); 
//    int byte_3 = spi.write(0x00); 
//    int byte_4 = spi.write(0x00); 
     
//    float temp = byte_3<<3; 
//    temp = ((temp/2047)*200)-50; 
//    float psi = byte_1<<8|byte_2; 
//    //psi = byte_1|byte_2; 
//    psi = ((psi-OUT_MIN)*5)/(OUT_MAX-OUT_MIN); 
     
//    pc.printf("Byte 1 = %X; Byte 2 = %X; Byte 3 = %X; Byte 4 = %X; PSI =  %.4f; TEMP = %.2f\r",byte_1,byte_2,byte_3,byte_4,psi,temp); 
     
//    // Deselect the device 
//    cs = 1; 
//    wait(0.5); 
//    } 
//}