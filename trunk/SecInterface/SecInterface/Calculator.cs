using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SecInterface
{
    class Calculator
    {
        private static float _rpm = 0;
        private static float _psi = 0;
        private static float _slope = 25f;       /// Assume PSI is Y axis, RPM is X axis

        public static float RPM
        {
            get { return Calculator._rpm; }
            set { Calculator._rpm = value; }
        }

        public static float PSI
        {
            get { return Calculator._psi; }
            set { Calculator._psi = value; }
        }

        public static float Slope
        {
            get { return Calculator._slope; }
            set { Calculator._slope = value; }
        }

        public static void Launch()
        {
            Thread updateThread = new Thread(new ThreadStart(SetTextboxText));
            updateThread.Start();
        }

        public static void SetTextboxText()
        {
            while (true)
            {
                Thread.Sleep(200);
                Program.ThinMintInterface.SetTextboxText();
            }
        }

        public static float GetProspectiveRPM(float psi)
        {
            return psi * Slope;
        }

        public static float GetProspectivePSI(float rpm)
        {
            return rpm * (1 / Slope);
        }
    }
}
