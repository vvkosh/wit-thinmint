using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SecInterface
{
    static class Program
    {
        private static Form1 _thinMintInterface;

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
            Calculator.Launch();
            Application.Run(ThinMintInterface);
        }
    }
}
