using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SecInterface
{
    static class Program
    {
        private static Form1 _thinMintInterface;
        private static float _rpm = 0;
        private static float _psi = 0;

        public static float RPM
        {
            get { return Program._rpm; }
            set { Program._rpm = value; }
        }

        public static float PSI
        {
            get { return Program._psi; }
            set { Program._psi = value; }
        }

        public static Form1 ThinMintInterface
        {
            get { return Program._thinMintInterface; }
            set { Program._thinMintInterface = value; }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ThinMintInterface = new Form1();
            SerialIO.Launch();
            Application.Run(ThinMintInterface);
        }
    }
}
