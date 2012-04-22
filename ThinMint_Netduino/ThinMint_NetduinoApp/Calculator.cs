using System;
using Microsoft.SPOT;

namespace ThinMint_Netduino
{
    class Calculator
    {
        private static float _slope;

        public static float Slope
        {
            get { return _slope; }
        }
    }
}
